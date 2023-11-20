using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HOST.Networking;
using UnityEngine.Events;
using FMETP;
using HOST.Monitoring.Settings;

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



        private int currentRiddleIndex = 0;

        [SerializeField]
        private bool startScenario = false;

        private void Start()
        {
            if (startScenario)
                StartScenario();
        }

        private void OnEnable()
        {

        }

        public void StartScenario()
        {
            if (!HostNetworkManager.instance.IsServer()) return;
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
            if (!HostNetworkManager.instance.IsServer()) return;
            riddles[currentRiddleIndex].StartRiddle();
        }

        public void OnRiddleComplete(Riddle r)
        {
            if (!HostNetworkManager.instance.IsServer()) return;
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

        public List<Riddle> GetRiddles()
        {
            return riddles;
        }

        public void PlayInfluence(int influenceLevel)
        {
            riddles[currentRiddleIndex].PlayInfluence(influenceLevel);
        }
    }
}