using System;
using UnityEngine;
using Newtonsoft.Json;


namespace HOST.Networking
{
    public enum HostNetworkMessageType
    {
        RPC,
        ObjectSync,
    }

    public enum HostNetworkTarget
    {
        Others,
        All,
        Server,
        Target,
    }


    [Serializable]
    public struct HostNetworkMessage
    {
        public HostNetworkMessageType MessageType;
        public HostNetworkTarget NetworkTarget;
        //public string SourceIP = "";
        //public string TargetIP = "";
        public string Data;
    }

    [Serializable]
    public struct HostNetworkObjectTransform
    {
        public int Id;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

    }

    public class HostNetworkRPCMessage
    {
        public int InstanceId { get; set; }
        public string MethodName { get; set; }
        public object[] Parameters { get; set; }

        public HostNetworkRPCMessage ConvertInt64ToInt32Payload()
        {
            if (Parameters != null && Parameters.Length != 0)
            {
                object[] convertedPayload = new object[Parameters.Length];

                for (int i = 0; i < Parameters.Length; i++)
                {
                    if (Parameters[i].GetType() == typeof(long))
                    {
                        convertedPayload[i] = Convert.ToInt32(Parameters[i]);
                    }
                    else
                    {
                        convertedPayload[i] = Parameters[i];
                    }
                }

                Parameters = convertedPayload;
            }

            return this;
        }
    }

    public class HostNetworkTools
    {

        public static string SerializeMessage(HostNetworkMessage message)
        {
            return JsonConvert.SerializeObject(message);
        }

        public static HostNetworkMessage DeserializeMessage(string data)
        {
            return JsonConvert.DeserializeObject<HostNetworkMessage>(data);
        }

        public static string SerializeObjectTransform(HostNetworkObjectTransform transform)
        {
            // JsonConvert and ignore loop references

            return JsonConvert.SerializeObject(transform, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            
        }

        public static HostNetworkObjectTransform DeserializeObjectTransform(string data)
        {
            return JsonConvert.DeserializeObject<HostNetworkObjectTransform>(data);
        }

        public static string SerializeRPCMessage(HostNetworkRPCMessage rpcMessage)
        {
            return JsonConvert.SerializeObject(rpcMessage);
        }

        public static HostNetworkRPCMessage DeserializeRPCMessage(string data)
        {
            return JsonConvert.DeserializeObject<HostNetworkRPCMessage>(data).ConvertInt64ToInt32Payload();
        }
    }
}