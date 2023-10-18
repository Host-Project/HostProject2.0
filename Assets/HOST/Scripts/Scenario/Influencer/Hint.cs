using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Scenario
{
    public class Hint : Influencer
    {
        [SerializeField]
        [Range(1, 5)]
        private int influenceLevel = -1;


        public override int GetInfluenceLevel()
        {
            return influenceLevel;
        }

        public void Debug()
        {
            UnityEngine.Debug.Log("Debug Hint of influence : " + GetInfluenceLevel());
        }
    }
}
