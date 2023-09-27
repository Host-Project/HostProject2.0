using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Networking.Transport
{
    public interface IHostNetworkListener
    {
        void OnConnected();
        void OnDisconnected();
        void OnDataReceived(int senderId, HostNetworkMessage data);
    }
}
