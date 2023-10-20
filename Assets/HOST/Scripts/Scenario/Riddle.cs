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

        public List<Element> Elements { get => elements; set => elements = value; }

        private new void Start()
        {
            base.Start();
            foreach (Element element in Elements)
            {
                element.onComplete.AddListener(OnElementComplete);
            }
        }

        private bool IsCompleted()
        {
            foreach (Element element in Elements)
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
            if (IsCompleted())
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

        public void PlayInfluence(int influenceLevel)
        {
            while (true)
            {
                foreach (Element e in Elements)
                {
                    if (e.IsCompleted()) continue;

                    foreach (Influencer influencer in e.influencers)
                    {
                        if (influencer.GetInfluenceLevel() == influenceLevel)
                        {
                            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                            {
                                InstanceId = influencer.InstanceId,
                                MethodName = "Play",
                                Parameters = new object[] { }
                            });

                            influencer.Play();
                            return;
                        }
                    }
                }
                influenceLevel += influenceLevel > 0 ? -1 : 1;
                if (influenceLevel == 0)
                    return;
            }
        }

    }

}