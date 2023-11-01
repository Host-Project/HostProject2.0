using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Influencers
{
    public abstract class Influencer : HostNetworkRPC
    {
        [SerializeField]
        private UnityEvent play;

        public abstract int GetInfluenceLevel();
        public void Play()
        {
            play.Invoke();
        }
        
    }
}