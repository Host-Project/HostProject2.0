using FMETP;
using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;
using HOST.Influencers;
using System.Linq;

namespace HOST.Scenario
{
    public class Riddle : HostNetworkRPC
    {
        [SerializeField]
        private List<Element> elements;

        public List<Influencer> influencers;

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

        public bool IsCompleted()
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
        

        public void StartRiddle()
        {
            if (HostNetworkManager.instance.IsServer())
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

        public List<Influencer> GetPossibleInfluencers()
        {
            List<Influencer> list = new List<Influencer>();
            foreach (Element e in Elements)
            {
                if(e.IsCompleted()) continue;

                list.AddRange(e.influencers.Where(i => i.IsPlayable()));
            }
            list.AddRange(influencers.Where(i => i.IsPlayable()));
            return list;
        }
        public void Complete()
        {
            foreach(Element e in Elements)
            {
                e.RequestComplete();
            }
        }

    }

}