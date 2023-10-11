using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HOST.Networking {
    public class HostNetworkRPC : MonoBehaviour
    {

        public static int instanceCount = 0;


        public int InstanceId { get; set; }

        public static List<HostNetworkRPC> rpcInstances = new List<HostNetworkRPC>();

        // Start is called before the first frame update
        protected void Start()
        {
            InstanceId = instanceCount++;
            rpcInstances.Add(this);
        }

        
    }
}