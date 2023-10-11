using FMETP;
using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Scenario
{
    public class Riddle : HostNetworkRPC
    {
        [SerializeField]
        private List<Element> elements;

        [SerializeField]
        private int weight;

        public UnityEvent<string> onGlobalStart;
        public UnityEvent<Riddle, string> onRiddleStart;
        public UnityEvent<Riddle> onRiddleComplete;

        private void Start()
        {
            base.Start();
            foreach (Element element in elements)
            {
                element.OnComplete.AddListener(OnElementComplete);
            }
        }

        private void OnElementComplete(Element elem)
        {
            Debug.Log(elem.name + " completed");
            foreach(Element element in elements)
            {
                if (!element.IsCompleted())
                {
                    return;
                }
            }
            onRiddleComplete.Invoke(this);
        }

        public void StartGlobal(string data)
        {
            if(FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
            {
                HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                {
                    InstanceId = this.InstanceId,
                    MethodName = "StartGlobal",
                    Parameters = new object[] { data }
                });
            }
            onGlobalStart.Invoke(data);
        }

        public void StartRiddle(string data)
        {
            if (FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
            {
                HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                {
                    InstanceId = this.InstanceId,
                    MethodName = "StartRiddle",
                    Parameters = new object[] { data }
                });
            }
            onRiddleStart.Invoke(this, data);
        }

    }

}