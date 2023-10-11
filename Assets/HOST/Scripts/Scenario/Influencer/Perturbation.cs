using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Scenario
{
    public class Perturbation : MonoBehaviour, IInfluencer
    {
        [SerializeField]
        [Range(-5,-1)]
        private int influenceLevel = -1;

        [SerializeField]
        private UnityEvent play;

        public int GetInfluenceLevel()
        {
            return influenceLevel;
        }

        public void Play()
        {
            play.Invoke();
        }

        public void Debug()
        {
            UnityEngine.Debug.Log("Debug Perturbation");
        }
    }
}