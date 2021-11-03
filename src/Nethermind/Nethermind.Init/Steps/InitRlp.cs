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

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nethermind.Api;
using Nethermind.Core.Attributes;
using Nethermind.Network;
using Nethermind.Serialization.Rlp;

namespace Nethermind.Init.Steps
{
    [RunnerStepDependencies(typeof(ApplyMemoryHint))]
    public class InitRlp : IStep
    {
        private readonly INethermindApi _api;

        public InitRlp(INethermindApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        [Todo(Improve.Refactor, "Automatically scan all the references solutions?")]
        public virtual Task Execute(CancellationToken _)
        {
            if (_api.SpecProvider == null) throw new StepDependencyException(nameof(_api.SpecProvider));
            if (_api.LogManager == null) throw new StepDependencyException(nameof(_api.LogManager));

            var logger = _api.LogManager.GetClassLogger();
            logger.Info("Start InitRLP");
            
           Rlp.RegisterDecoders(Assembly.GetAssembly(typeof(NetworkNodeDecoder)));
           logger.Info($"InitRLP, HeaderDecoder: {HeaderDecoder.Eip1559TransitionBlock} GenesisSpec: {_api.SpecProvider.GenesisSpec.Eip1559TransitionBlock}");
           HeaderDecoder.Eip1559TransitionBlock = _api.SpecProvider.GenesisSpec.Eip1559TransitionBlock;
           logger.Info($"End InitRLP, HeaderDecoder: {HeaderDecoder.Eip1559TransitionBlock}");
           return Task.CompletedTask;
        }
    }
}
