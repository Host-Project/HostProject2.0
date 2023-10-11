using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Scenario
{
    public class Hint : MonoBehaviour, IInfluencer
    {
        [SerializeField]
        [Range(1, 5)]
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

        public static void Debug()
        {
            UnityEngine.Debug.Log("Debug Hint");
        }
    }
}
