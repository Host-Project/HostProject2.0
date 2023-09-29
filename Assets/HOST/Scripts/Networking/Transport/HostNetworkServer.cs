/*
 *  Author : Quentin Forestier
 *  Date : 25.09.2023
 *  Last modification : 26.09.2023
 *  Comments :
 *  This file is used for the server side of the network.
 *  A lot of the code is taken from the Unity documentation :
 *  https://docs.unity3d.com/Packages/com.unity.transport@2.0/manual/client-server-simple.html
 */

using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking;
using System.Net.NetworkInformation;

namespace HOST.Networking.Transport
{
    public class HostNetworkServer : MonoBehaviour, IHostNetworkActor
    {
        

        [SerializeField]
        private ushort port = 7777;

        [SerializeField]
        private TransportType transportType = TransportType.UDP;

        private  List<IHostNetworkListener> hostNetworkListeners = new List<IHostNetworkListener>();


        // primary API to the underlying network transport driver
        NetworkDriver m_Driver;

        // list to hold all the connections
        NativeList<NetworkConnection> m_Connections;


        void Start()
        {


            m_Driver = NetworkTools.GetNetworkDriver(transportType);

            m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(port);
            if (m_Driver.Bind(endpoint) != 0)
            {
                Debug.LogError("[HOST - Server Start] Failed to bind to port " + port + ".");
                return;
            }
            m_Driver.Listen();

            Debug.Log("[HOST - Server Start] Server listening on port " + port + "...");
        }

        void OnDestroy()
        {
            if (m_Driver.IsCreated)
            {
                m_Driver.Dispose();
                m_Connections.Dispose();
            }
            Debug.Log("[HOST - Server OnDestroy] Server shut down.");
        }

        void Update()
        {
            m_Driver.ScheduleUpdate().Complete();

            CleanConnections();

            AcceptConnections();

            HandleEvents();
        }

        private void CleanConnections()
        {
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (!m_Connections[i].IsCreated)
                {
                    m_Connections.RemoveAtSwapBack(i);
                    i--;
                }
            }
        }

        private void AcceptConnections()
        {
            NetworkConnection c;
            while ((c = m_Driver.Accept()) != default)
            {
                m_Connections.Add(c);
                Debug.Log("[HOST - Server] Accepted a connection.");

                foreach(IHostNetworkListener listener in hostNetworkListeners)
                {
                    listener.OnConnected();
                }
            }
        }

        private void HandleEvents()
        {
            for (int i = 0; i < m_Connections.Length; i++)
            {
                DataStreamReader stream;
                NetworkEvent.Type cmd;
                while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
                {
                    Debug.Log("[HOST - ServerUpdateReceiveJob] Received command : " + cmd.ToString());
                    switch (cmd)
                    {
                        case NetworkEvent.Type.Disconnect:
                            HandleDisconnect(i);
                            break;
                        case NetworkEvent.Type.Data:
                            HandleData(i, stream);
                            break;
                    }

                }
            }
        }

        private void HandleData(int senderId, DataStreamReader stream)
        {
            Debug.Log("[HOST - HostNetworkServer - HandleData] Received data from client.");
            foreach(IHostNetworkListener listener in hostNetworkListeners)
            {
                listener.OnDataReceived(senderId, NetworkTools.ReadData(stream));
            }   
        }

        private void HandleDisconnect(int i)
        {
            Debug.Log("[HOST - HostNetworkServer - HandleDisconnect] Client disconnected from the server.");
            m_Connections[i] = default;
            foreach(IHostNetworkListener listener in hostNetworkListeners)
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

        /// <summary>
        /// Send data to specified targets. If no targets are specified, send to all connections.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="connectionIds"></param>
        public void SendData(HostNetworkMessageType type, object data, List<int> connectionIds = null)
        {
            List<int> targets = connectionIds != null && connectionIds.Count > 0 ? connectionIds : Enumerable.Range(0, m_Connections.Length).ToList();
            foreach (int connectionId in targets)
            {
                NetworkTools.SendData(m_Driver, m_Connections[connectionId], type, data);
            }
        }
    }
}