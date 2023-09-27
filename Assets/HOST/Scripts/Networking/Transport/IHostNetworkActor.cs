using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.Collections;

namespace HOST.Networking.Transport
{
    public interface IHostNetworkActor
    {
        void RegisterListener(IHostNetworkListener listener);
        void UnregisterListener(IHostNetworkListener listener);
        void SendData(HostNetworkMessageType type, object data, List<int> connectionIds= null );
        
    }
}