using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HOST.Networking;
using UnityEngine.Events;
using FMETP;

namespace HOST.Scenario
{
    public class Scenario : HostNetworkRPC
    {
        [SerializeField]
        private List<Riddle> riddles;

        #region Events

        public UnityEvent onScenarioStart;

        public UnityEvent onScenarioComplete;

        #endregion

        public static Scenario instance;


        private int currentRiddleIndex = 0;

        private new void Start()
        {
            base.Start();
            if (instance == null) instance = this;
        }

        private void Update()
        {


        }

        public void StartScenario()
        {
            if (FMNetworkManager.instance.NetworkType == FMNetworkType.Client) return;
            foreach (Riddle riddle in riddles)
            {
                riddle.onRiddleComplete.AddListener(OnRiddleComplete);
                riddle.StartGlobal();
            }
            currentRiddleIndex = 0;
            StartRiddle();
            onScenarioStart.Invoke();
        }

        public void StartRiddle()
        {
            if (FMNetworkManager.instance.NetworkType == FMNetworkType.Client) return;
            riddles[currentRiddleIndex].StartRiddle();
        }

        public void OnRiddleComplete(Riddle r)
        {
            if (FMNetworkManager.instance.NetworkType == FMNetworkType.Client) return;
            if (r != riddles[currentRiddleIndex]) return;
            if (currentRiddleIndex == riddles.Count - 1)
            {

                onScenarioComplete.Invoke();
            }
            else
            {
                currentRiddleIndex++;
                StartRiddle();
            }
        }


    }
}