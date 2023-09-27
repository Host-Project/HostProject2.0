/**
 * Author : Quentin Forestier
 * Date : 26.09.2023
 * Last modification : 26.09.2023
 * Comments : 
 * This file is used to place common network transport tools.
 */
using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace HOST.Networking.Transport
{

    public enum HostNetworkMessageType
    {
        RPC,
        Sync,
    }

    public class HostNetworkMessage
    {
        public HostNetworkMessageType MessageType;
        //public HostNetworkTarget NetworkTarget;
        //public string SourceIP = "";
        //public string TargetIP = "";
        public string Data = "";
    }

    public class HostNetworkMessageEvent : EventArgs
    {
        public HostNetworkMessage Message { get; set; }

        public HostNetworkMessageEvent(HostNetworkMessage message)
        {
            Message = message;
        }

    }
    static class NetworkTools
    {
        public static NetworkDriver GetNetworkDriver(TransportType transportType)
        {
            switch (transportType)
            {
                case TransportType.WebSocket:
                    return NetworkDriver.Create(new WebSocketNetworkInterface());
                case TransportType.UDP:
                    return NetworkDriver.Create(new UDPNetworkInterface());
                default:
                    throw new System.Exception("Invalid transport type.");
            }
        }

        public static void SendData(NetworkDriver driver, NetworkConnection connection, HostNetworkMessageType type, object data)
        {
            

            HostNetworkMessage message = new HostNetworkMessage()
            {
                MessageType = type,
                Data = JsonUtility.ToJson(data),

            };
            string dataString = JsonUtility.ToJson(message);

            if (System.Text.Encoding.Unicode.GetByteCount(dataString) > 512)
            {
                Debug.LogError("[HOST - Server] Data is too big to be sent.");
                return;
            }
            driver.BeginSend(connection, out var writer);
            writer.WriteFixedString512(new FixedString512Bytes(dataString));
            driver.EndSend(writer);
        }

        public static HostNetworkMessage ReadData(DataStreamReader reader)
        {
            return JsonUtility.FromJson<HostNetworkMessage>(reader.ReadFixedString512().ToString());
        }
    }
    enum TransportType
    {
        WebSocket,
        UDP
    }
}
