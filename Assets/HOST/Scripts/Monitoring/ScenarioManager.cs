using FMETP;
using HOST;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Monitoring
{
    [Serializable]
    public struct WeightedRiddle
    {
        public Scenario.Riddle riddle;
        public int weight;
    }
    public class ScenarioManager : ProgressionStateManager
    {
        public TMP_Text timerText;

        public List<WeightedRiddle> riddles; // Automatically updated by the editor

        public UnityEvent<float> onTimerTick;


        private void Start()
        {

            foreach (WeightedRiddle wr in riddles)
            {
                wr.riddle.onRiddleStart.AddListener(OnRiddleStart);
                wr.riddle.onRiddleComplete.AddListener(OnRiddleComplete);
                foreach (Scenario.Element element in wr.riddle.Elements)
                {
                    element.onComplete.AddListener(OnElementComplete);
                }
                ScenarioWeights += wr.weight;
            }
        }

        private void OnRiddleComplete(Scenario.Riddle r)
        {
            WeightedRiddle rw = riddles.Where(w => w.riddle == r).First();

            ScenarioProgression += rw.weight;
        }

        private void OnRiddleStart(Scenario.Riddle r)
        {
            WeightedRiddle rw = riddles.Where(w => w.riddle == r).First();

            CurrentRiddleWeight = rw.weight;
            CurrentRiddleProgression = 0.0f;
            CurrentRiddleDuration = 0.0f;

        }

        private void OnElementComplete(Scenario.Element e)
        {
            CurrentRiddleProgression += 1.0f / CurrentRiddleWeight;
            LastCompletionTime = CurrentTime;
        }

        public void StartTimer()
        {
            if (FMNetworkManager.instance.NetworkType == FMNetworkType.Client) return;
            InvokeRepeating("TimerTick", 0.0f, 1.0f);
        }

        public void StopTimer()
        {
            CancelInvoke("TimerTick");
        }

        private void TimerTick()
        {
            CurrentTime += 1.0f;
            CurrentRiddleDuration += 1.0f;


            int influenceLevel = ComputeInfluenceLevel();
            if (influenceLevel != 0)
            {
                Scenario.Scenario.instance.PlayInfluence(influenceLevel);
            }

            onTimerTick.Invoke(CurrentTime);
            timerText.text = "Time : " + CurrentTime.ToString();
        }

        public float GetTime()
        {
            return CurrentTime;
        }



    }
}