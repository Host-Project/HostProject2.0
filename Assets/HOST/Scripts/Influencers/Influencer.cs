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

        [SerializeField]
        [Tooltip("The number of showings of the influencer. -1 means infinite.")]
        private int numberOfShowings = 1;

        public int NumberOfShowings { get => numberOfShowings; set => numberOfShowings = value; }
        public abstract int GetInfluenceLevel();
        public void Play()
        {
            if (numberOfShowings > 0)
            {
                numberOfShowings--;
            }
            play.Invoke();
        }
        
        public bool IsPlayable()
        {
            return numberOfShowings != 0;
        }
    }
}