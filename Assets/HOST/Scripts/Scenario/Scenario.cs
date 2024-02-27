using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HOST.Networking;
using UnityEngine.Events;
using FMETP;
using HOST.Monitoring.Settings;
using HOST.Influencers;
using System.Linq;

namespace HOST.Scenario
{
    public class Scenario : HostNetworkRPC
    {
        [SerializeField]
        private List<Riddle> riddles;

        [SerializeField]
        private List<Influencer> influencers;
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


        public void StartScenario()
        {
            if (!HostNetworkManager.instance.IsServer()) return;
            foreach (Riddle riddle in riddles)
            {
                riddle.onRiddleComplete.AddListener(OnRiddleComplete);
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
            List<Influencer> possibleInfluencer = new List<Influencer>();

            possibleInfluencer.AddRange(influencers.Where(i => i.IsPlayable()));
            possibleInfluencer.AddRange(riddles[currentRiddleIndex].GetPossibleInfluencers());

            while (true)
            {

                foreach (Influencer influencer in possibleInfluencer)
                {
                    if (influencer.GetInfluenceLevel() == influenceLevel)
                    {
                        HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                        {
                            InstanceId = influencer.InstanceId,
                            MethodName = "Play",
                            Parameters = new object[] { }
                        });

                        influencer.Play();
                        return;
                    }
                }
                influenceLevel += influenceLevel > 0 ? -1 : 1;
                if (influenceLevel == 0)
                    return;
            }
        }

        public void CompleteCurrentRiddle()
        {
            riddles[currentRiddleIndex].Complete();
        }
    }
}