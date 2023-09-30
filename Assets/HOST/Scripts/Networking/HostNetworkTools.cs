
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Networking
{
    public  enum HostNetworkMessageType
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



    public struct HostNetworkMessage
    {
        public HostNetworkMessageType MessageType;
        public HostNetworkTarget NetworkTarget;
        //public string SourceIP = "";
        //public string TargetIP = "";
        public string Data;
    }

    public struct HostNetworkObjectTransform
    {
        public int Id;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

    }

    public class HostNetworkTools
    {
        public static string SerializeMessage(HostNetworkMessage message)
        {
            return JsonUtility.ToJson(message);
        }

        public static HostNetworkMessage DeserializeMessage(string data)
        {
            return JsonUtility.FromJson<HostNetworkMessage>(data);
        }

        public static string SerializeObjectTransform(HostNetworkObjectTransform transform)
        {
            return JsonUtility.ToJson(transform);
        }

        public static HostNetworkObjectTransform DeserializeObjectTransform(string data)
        {
            return JsonUtility.FromJson<HostNetworkObjectTransform>(data);
        }
    }
}