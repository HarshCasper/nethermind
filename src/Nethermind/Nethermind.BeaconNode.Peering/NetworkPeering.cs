﻿using System;
using System.Threading.Tasks;
using Nethermind.BeaconNode.Services;
using Nethermind.Core2.Containers;

namespace Nethermind.BeaconNode.Peering
{
    public class NetworkPeering : INetworkPeering
    {
        public Task PublishBeaconBlockAsync(BeaconBlock beaconBlock)
        {
            // Validate signature before broadcasting
            
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }
    }
}