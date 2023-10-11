using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HOST.Networking;
using UnityEngine.Events;

namespace HOST.Scenario
{
    public class Scenario : HostNetworkRPC
    {
        [SerializeField]
        private List<Riddle> riddles;

        [SerializeField]
        private int recommandedTimeInSeconds;

        [SerializeField]
        private UnityEvent onScenarioStart;

        [SerializeField]
        private UnityEvent onScenarioComplete;

        [SerializeField]
        private UnityEvent<Riddle> onRiddleComplete;

        public static Scenario instance;

        private int timeCounterInSeconds = 0;

        private int currentRiddleIndex = 0;

        private new void Start()
        {
            base.Start();
            if (instance == null) instance = this;
            foreach (Riddle element in riddles)
            {
                element.OnComplete.AddListener(OnRiddleComplete);
                element.StartGlobal();
            }
            onScenarioStart.Invoke();
        }

        private void Update()
        {
            

        }

        private void OnRiddleComplete()
        {

            if(currentRiddleIndex == riddles.Count)
            {
                onScenarioComplete.Invoke();
            }
            else
            {
                StartNextRiddle();
            }

        }

        private void StartNextRiddle()
        {
            currentRiddleIndex++;
            riddles[currentRiddleIndex].StartRiddle();
        }

        public float GetCompletion()
        {
            return 0f;
        }
    }
}