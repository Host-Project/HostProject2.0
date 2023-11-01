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

        public static int instanceCount = 0;


        public int InstanceId { get; set; }

        public static List<HostNetworkRPC> rpcInstances = new List<HostNetworkRPC>();

        // Start is called before the first frame update
        protected virtual void Start()
        {
            InstanceId = instanceCount++;
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


       
    }
}