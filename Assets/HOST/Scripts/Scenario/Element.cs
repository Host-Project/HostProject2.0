using FMETP;
using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Scenario
{
    public class Element : HostNetworkRPC
    {

        [SerializeField]
        public List<Influencer> influencers;

        public UnityEvent<Element> onComplete;

        private bool isCompleted = false;

        public void Complete()
        {
            if(!isCompleted)
            {
                HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                {
                    InstanceId = this.InstanceId,
                    MethodName = "Complete",
                    Parameters = new object[] {  }
                });
            }
            isCompleted = true;
            onComplete.Invoke(this);
        }


        public bool IsCompleted()
        {
            return isCompleted;
        }



    }

}
