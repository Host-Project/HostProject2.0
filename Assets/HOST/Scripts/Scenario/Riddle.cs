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


        public UnityEvent onGlobalStart;
        public UnityEvent<Riddle> onRiddleStart;
        public UnityEvent<Riddle> onRiddleComplete;

        private void Start()
        {
            base.Start();
            foreach (Element element in elements)
            {
                element.onComplete.AddListener(OnElementComplete);
            }
        }

        private bool IsCompleted()
        {
            foreach (Element element in elements)
            {
                if (!element.IsCompleted())
                {
                    return false;
                }
            }
            return true;
        }

        private void OnElementComplete(Element elem)
        {
            if(IsCompleted())
                onRiddleComplete.Invoke(this);
        }

        public void StartGlobal()
        {
            if (FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
            {
                HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                {
                    InstanceId = this.InstanceId,
                    MethodName = "StartGlobal",
                    Parameters = new object[] { }
                });
            }
            onGlobalStart.Invoke();
        }

        public void StartRiddle()
        {
            if (FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
            {
                HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                {
                    InstanceId = this.InstanceId,
                    MethodName = "StartRiddle",
                    Parameters = new object[] { }
                });
            }
            onRiddleStart.Invoke(this);
            if (IsCompleted())
            {
                onRiddleComplete.Invoke(this);
            }
        }

    }

}