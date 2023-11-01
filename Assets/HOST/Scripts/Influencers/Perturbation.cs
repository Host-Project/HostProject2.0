using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Influencers
{
    public class Perturbation : Influencer
    {
        [SerializeField]
        [Range(-5,-1)]
        private int influenceLevel = -1;

        public void Debug()
        {
            UnityEngine.Debug.Log("Debug Perturbation of influence : " + GetInfluenceLevel());
        }

        public override int GetInfluenceLevel()
        {
            return influenceLevel;
        }
    }
}