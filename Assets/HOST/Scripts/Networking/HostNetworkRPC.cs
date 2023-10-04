using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HOST.Networking {
    public class HostNetworkRPC : MonoBehaviour
    {

        public static int instanceCount = 0;
        public int instanceId = 0;

        public static List<HostNetworkRPC> rpcInstances = new List<HostNetworkRPC>();

        // Start is called before the first frame update
        protected void Start()
        {
            
            instanceId = instanceCount++;
            rpcInstances.Add(this);
        }

        
    }
}