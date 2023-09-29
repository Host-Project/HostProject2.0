/*
 *  Author : Quentin Forestier
 *  Date : 25.09.2023
 *  Last modification : 25.09.2023
 *  Comments :
 *  This file is used for the client side of the network.
 *  A lot of the code is taken from the Unity documentation :
 *  https://docs.unity3d.com/Packages/com.unity.transport@2.0/manual/client-server-jobs.html
 */

using UnityEngine;
using Unity.Networking.Transport;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Net;

namespace HOST.Networking.Transport
{

    public class HostNetworkClient : MonoBehaviour, IHostNetworkActor
    {
        [SerializeField]
        private ushort port = 7777;

        [SerializeField]
        private TransportType transportType = TransportType.UDP;

        private List<IHostNetworkListener> hostNetworkListeners = new List<IHostNetworkListener>();

        NetworkDriver m_Driver;
        NetworkConnection m_Connection;




        void Start()
        {
            m_Driver = NetworkTools.GetNetworkDriver(transportType);
            
            var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(port);
            m_Connection = m_Driver.Connect(endpoint);
        }

        void OnDestroy()
        {
            m_Driver.Dispose();
        }

        void Update()
        {
            m_Driver.ScheduleUpdate().Complete();

            if (!m_Connection.IsCreated)
            {
                return;
            }

            HandleEvents();
        }

        private void HandleEvents()
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
            {
                switch (cmd)
                {
                    case NetworkEvent.Type.Connect:
                        HandleConnection();
                        break;
                    case NetworkEvent.Type.Data:
                        HandleData(stream);
                        break;
                    case NetworkEvent.Type.Disconnect:
                        HandleDisconnect();
                        break;

                }

            }
        }

        private void HandleConnection()
        {
            Debug.Log("[HOST - HostNetworkClient - HandleConnection] Connected to the server.");
            foreach (IHostNetworkListener listener in hostNetworkListeners)
            {
                listener.OnConnected();
            }
        }

        private void HandleData(DataStreamReader stream)
        {
            Debug.Log("[HOST - HostNetworkServer - HandleData] Received data from client.");
            foreach (IHostNetworkListener listener in hostNetworkListeners)
            {
                listener.OnDataReceived(0, NetworkTools.ReadData(stream));
            }
        }
        private void HandleDisconnect()
        {
            Debug.Log("[HOST - HostNetworkClient - HandleDisconnect] Client disconnected from the server.");
            m_Connection = default;
            foreach (IHostNetworkListener listener in hostNetworkListeners)
            {
                listener.OnDisconnected();
            }
        }

        public void RegisterListener(IHostNetworkListener listener)
        {
            hostNetworkListeners.Add(listener);
        }

        public void UnregisterListener(IHostNetworkListener listener)
        {
            hostNetworkListeners.Remove(listener);
        }

        public void SendData(HostNetworkMessageType type, object data, List<int> connectionIds = null)
        {
            NetworkTools.SendData(m_Driver, m_Connection,type, data);
        }


    }
}