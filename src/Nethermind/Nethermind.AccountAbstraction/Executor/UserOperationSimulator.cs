//  Copyright (c) 2021 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nethermind.Abi;
using Nethermind.AccountAbstraction.Data;
using Nethermind.AccountAbstraction.Source;
using Nethermind.Blockchain;
using Nethermind.Blockchain.Contracts.Json;
using Nethermind.Blockchain.Processing;
using Nethermind.Blockchain.Receipts;
using Nethermind.Blockchain.Rewards;
using Nethermind.Blockchain.Tracing;
using Nethermind.Blockchain.Validators;
using Nethermind.Consensus;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Core.Specs;
using Nethermind.Crypto;
using Nethermind.Db;
using Nethermind.Evm.Tracing;
using Nethermind.Evm.TransactionProcessing;
using Nethermind.Int256;
using Nethermind.JsonRpc;
using Nethermind.Logging;
using Nethermind.State;
using Nethermind.Trie.Pruning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nethermind.AccountAbstraction.Executor
{
    public class UserOperationSimulator : IUserOperationSimulator
    {
        private readonly IStateProvider _stateProvider;
        private readonly ISigner _signer;
        private readonly IAccountAbstractionConfig _config;
        private readonly Address _create2FactoryAddress;
        private readonly Address _singletonAddress;
        private readonly ISpecProvider _specProvider;
        private readonly IBlockTree _blockTree;
        private readonly IReadOnlyDbProvider _dbProvider;
        private readonly IReadOnlyTrieStore _trieStore;
        private readonly ILogManager _logManager;
        private readonly IBlockPreprocessorStep _recoveryStep;

        private AbiDefinition _contract;
        private IAbiEncoder _abiEncoder;

        public UserOperationSimulator(
            IStateProvider stateProvider,
            ISigner signer,
            IAccountAbstractionConfig config,
            Address create2FactoryAddress,
            Address singletonAddress,
            ISpecProvider specProvider,
            IBlockTree blockTree,
            IDbProvider dbProvider,
            IReadOnlyTrieStore trieStore,
            ILogManager logManager,
            IBlockPreprocessorStep recoveryStep)
        {
            _stateProvider = stateProvider;
            _signer = signer;
            _config = config;
            _create2FactoryAddress = create2FactoryAddress;
            _singletonAddress = singletonAddress;
            _specProvider = specProvider;
            _blockTree = blockTree;
            _dbProvider = dbProvider.AsReadOnly(false);
            _trieStore = trieStore;
            _logManager = logManager;
            _recoveryStep = recoveryStep;
            
            _abiEncoder = new AbiEncoder();

            using (StreamReader r = new StreamReader("Contracts/EntryPoint.json"))
            {
                string json = r.ReadToEnd();
                JObject obj = JObject.Parse(json);
                
                _contract = LoadContract(obj);
            }
        }

        public Transaction BuildTransactionFromUserOperations(IEnumerable<UserOperation> userOperations, BlockHeader parent, IReleaseSpec spec)
        {
            byte[] computedCallData;
            long gasLimit;
            
            UserOperation[] userOperationArray = userOperations.ToArray();
            if (userOperationArray.Length == 1)
            {
                UserOperation userOperation = userOperationArray[0];
                
                AbiSignature abiSignature = _contract.Functions["handleOp"].GetCallInfo().Signature;
                computedCallData = _abiEncoder.Encode(
                    AbiEncodingStyle.IncludeSignature,
                    abiSignature,
                    userOperation.Abi, _signer.Address);

                gasLimit = (long)userOperation.VerificationGas + (long)userOperation.CallGas + 100000; // TODO WHAT CONSTANT
            }
            else
            {
                AbiSignature abiSignature = _contract.Functions["handleOps"].GetCallInfo().Signature;
                computedCallData = _abiEncoder.Encode(
                    AbiEncodingStyle.IncludeSignature,
                    abiSignature,
                    userOperationArray.Select(op => op.Abi).ToArray(), _signer.Address);
            
                gasLimit = userOperationArray.Aggregate((long)0,
                    (sum, operation) => sum + (long)operation.VerificationGas + (long)operation.CallGas + 100000); // TODO WHAT CONSTANT
            }
            
            Transaction transaction = BuildTransaction(gasLimit, computedCallData, _signer.Address, parent, spec, false);
            
            return transaction;
        }

        public Task<ResultWrapper<Keccak>> Simulate(
            UserOperation userOperation, 
            BlockHeader parent,
            CancellationToken cancellationToken = default, 
            UInt256? timestamp = null)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            
            IReleaseSpec currentSpec = _specProvider.GetSpec(parent.Number + 1);
            ReadOnlyTxProcessingEnv txProcessingEnv = new(_dbProvider, _trieStore, _blockTree, _specProvider, _logManager);
            ITransactionProcessor transactionProcessor = txProcessingEnv.Build(_stateProvider.StateRoot);
            
            Transaction simulateValidationTransaction = BuildSimulateValidationTransaction(userOperation, parent, currentSpec);
            (bool validationSuccess, UserOperationAccessList validationAccessList, string? error) =
                SimulateValidation(simulateValidationTransaction, userOperation, parent, transactionProcessor);

            stopwatch.Stop();
            _logManager.GetClassLogger().Info($"AA: validation for op {userOperation.Hash} completed in {stopwatch.ElapsedMilliseconds}ms");
            
            if (!validationSuccess)
            {
                return Task.FromResult(ResultWrapper<Keccak>.Fail(error ?? "unknown simulation failure"));
            }

            if (userOperation.AlreadySimulated)
            {
                if (!UserOperationAccessList.AccessListContains(userOperation.AccessList.Data,
                    validationAccessList.Data))
                {
                    return Task.FromResult(ResultWrapper<Keccak>.Fail("access list exceeded"));
                }
            }
            else
            {
                userOperation.AccessList = validationAccessList;
                userOperation.AlreadySimulated = true;
            }
            
            return Task.FromResult(ResultWrapper<Keccak>.Success(userOperation.Hash));
        }
        
        private (bool success, UserOperationAccessList accessList, string? error) SimulateValidation(Transaction transaction, UserOperation userOperation, BlockHeader parent, ITransactionProcessor transactionProcessor)
        {
            UserOperationBlockTracer blockTracer = CreateBlockTracer(parent, userOperation);
            ITxTracer txTracer = blockTracer.StartNewTxTrace(transaction);
            transactionProcessor.CallAndRestore(transaction, parent, txTracer);
            blockTracer.EndTxTrace();

            string? error = null;
            
            if (!blockTracer.Success)
            {
                if (blockTracer.FailedOp is not null)
                {
                    error = blockTracer.FailedOp.ToString()!;
                }
                else
                {
                    error = blockTracer.Error;
                }
                return (false, UserOperationAccessList.Empty, error);
            }
            
            UserOperationAccessList userOperationAccessList = new UserOperationAccessList(blockTracer.AccessedStorage);

            return (true, userOperationAccessList, error);
        }

        private Transaction BuildSimulateValidationTransaction(
            UserOperation userOperation, 
            BlockHeader parent,
            IReleaseSpec spec)
        {
            AbiSignature abiSignature = _contract.Functions["simulateValidation"].GetCallInfo().Signature;
            UserOperationAbi userOperationAbi = userOperation.Abi;
            
            byte[] computedCallData = _abiEncoder.Encode(
                AbiEncodingStyle.IncludeSignature,
                abiSignature,
                userOperationAbi);

            Transaction transaction = BuildTransaction((long)userOperation.VerificationGas, computedCallData, Address.Zero, parent, spec, true);
            
            return transaction;
        }

        private Transaction BuildTransaction(long gaslimit, byte[] callData, Address sender, BlockHeader parent, IReleaseSpec spec, bool systemTransaction)
        {
            Transaction transaction = systemTransaction ? new SystemTransaction() : new Transaction();

            UInt256 fee = BaseFeeCalculator.Calculate(parent, spec);
            
            transaction.GasPrice = fee;
            transaction.GasLimit = gaslimit;
            transaction.To = _singletonAddress;
            transaction.ChainId = _specProvider.ChainId;
            transaction.Nonce = _stateProvider.GetNonce(_signer.Address);
            transaction.Value = 0;
            transaction.Data = callData;
            transaction.Type = TxType.EIP1559;
            transaction.DecodedMaxFeePerGas = fee;
            transaction.SenderAddress = sender;
            
            if (!systemTransaction) _signer.Sign(transaction);
            transaction.Hash = transaction.CalculateHash();

            return transaction;
        }

        private UserOperationBlockTracer CreateBlockTracer(BlockHeader parent, UserOperation userOperation) =>
            new(parent.GasLimit, userOperation, _stateProvider, _contract, _create2FactoryAddress, _singletonAddress, _logManager.GetClassLogger());
        
        private AbiDefinition LoadContract(JObject obj)
        {
            AbiDefinitionParser parser = new();
            parser.RegisterAbiTypeFactory(new AbiTuple<UserOperationAbi>());
            AbiDefinition contract = parser.Parse(obj["abi"]!.ToString());
            AbiTuple<UserOperationAbi> userOperationAbi = new();
            return contract;
        }
    }
}
