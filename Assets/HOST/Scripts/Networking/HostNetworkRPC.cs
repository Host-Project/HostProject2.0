using FMETP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HOST.Networking
{
    public class HostNetworkRPC : MonoBehaviour
    {
        [SerializeField]
        private int instanceId = -1;
        public static int instanceCount = 0;
        

        public static List<HostNetworkRPC> rpcInstances = new List<HostNetworkRPC>();

        public int InstanceId { get => instanceId; set => instanceId = value; }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            /*if(InstanceId == -1 || GetRPCInstance(InstanceId) != null)
            {
                throw new Exception("This RPC Id is not registered or already in use");
            }*/
            rpcInstances.Add(this);
        }

        public void HandleRPC(HostNetworkRPCMessage message)
        {
            Debug.Log("Handle RPC");
            MethodInfo method = this.GetType().GetMethod(message.MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);


            if (method.GetParameters().Length > message.Parameters.Length)
            {
                message.Parameters = message.Parameters.Append(FMNetworkManager.instance.NetworkType == FMNetworkType.Server).ToArray();
            }

            method.Invoke(this, message.Parameters);
        }

        public static HostNetworkRPC GetRPCInstance(int instanceId)
        {
            return rpcInstances.Find(x => x.InstanceId == instanceId);
        }
       
    }
}