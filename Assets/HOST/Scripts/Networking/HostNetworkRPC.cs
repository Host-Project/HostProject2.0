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
            rpcInstances.Add(this);
        }

        private void OnValidate()
        {
            if(GetRPCInstance(instanceId) != null)
            {
                throw new Exception("Instance ID already exists");
            }
        }

        private void OnDestroy()
        {
            rpcInstances.Remove(this);
        }

        public void HandleRPC(HostNetworkRPCMessage message)
        {
            MethodInfo method = this.GetType().GetMethod(message.MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);


            if (method.GetParameters().Length > message.Parameters.Length)
            {
                message.Parameters = message.Parameters.Append(HostNetworkManager.instance.IsServer()).ToArray();
            }

            method.Invoke(this, message.Parameters);
        }

        public static HostNetworkRPC GetRPCInstance(int instanceId)
        {
            return rpcInstances.Find(x => x.InstanceId == instanceId);
        }

        public static void DebugRPCInstances()
        {
            foreach (HostNetworkRPC instance in rpcInstances)
            {
                Debug.Log(instance);
                Debug.Log(string.Format("ID : {0}, Name : {1}", instance.InstanceId, instance.gameObject.name));
            }
        }

    }
}