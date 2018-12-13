﻿/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Net;
using Nethermind.Core.Crypto;
using Nethermind.Core.Logging;
using Nethermind.Core.Model;
using Nethermind.Stats.Model;
using NLog;
using ILogger = Nethermind.Core.Logging.ILogger;

namespace Nethermind.Stats
{
    public class NodeFactory : INodeFactory
    {
        private readonly ILogger _logger;

        public NodeFactory(ILogManager logManager)
        {
            _logger = logManager.GetClassLogger<NodeFactory>();
        }
        
        public Node CreateNode(NodeId id, IPEndPoint address)
        {
            var node = new Node(id)
            {
                IsDiscoveryNode = false
            };
            
            node.InitializeAddress(address);
            return node;
        }

        public Node CreateNode(NodeId id, string host, int port, bool isDiscovery = false)
        {
            var node = new Node(id)
            {
                IsDiscoveryNode = isDiscovery
            };

            try
            {
                node.InitializeAddress(host, port);
            }
            catch (Exception e)
            {
                if(_logger.IsError) _logger.Error($"Unable to create node for host: {host}, port: {port} - {e.Message}");
            }
            
            return node;
        }

        public Node CreateNode(string host, int port)
        {
            Keccak512 socketHash = Keccak512.Compute($"{host}:{port}"); 
            var node = new Node(new NodeId(new PublicKey(socketHash.Bytes)))
            {
                IsDiscoveryNode = true
            };
            
            node.InitializeAddress(host, port);
            return node;
        }
    }
}