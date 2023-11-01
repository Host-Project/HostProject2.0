using FMETP;
using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HOST.Influencers;

namespace HOST.Scenario
{
    public class Element : HostNetworkRPC
    {

        [SerializeField]
        public List<Influencer> influencers;

        public UnityEvent<Element> onComplete;

        private bool isCompleted = false;


        public void RequestComplete()
        {
            if (FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
            {
                HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                {
                    InstanceId = this.InstanceId,
                    MethodName = "Complete",
                    Parameters = new object[] { }
                });
                Complete();
            }
            else
            {
                HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                {
                    InstanceId = this.InstanceId,
                    MethodName = "RequestComplete",
                    Parameters = new object[] { }
                });
            }
        }
        private void Complete()
        {
            isCompleted = true;
            onComplete.Invoke(this);
        }


        public bool IsCompleted()
        {
            return isCompleted;
        }



    }

}
