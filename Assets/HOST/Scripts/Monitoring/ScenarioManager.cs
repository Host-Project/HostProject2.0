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
    public class WeightedRiddle
    {
        public Scenario.Riddle riddle;
        public List<WeightedElements> weightedElements;

        public int GetWeight()
        {
            return weightedElements.Sum(w => w.weight);
        }
    }

    [Serializable]
    public class WeightedElements
    {
        public Scenario.Element element;
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
                ScenarioWeights += wr.GetWeight();
            }
        }

        private void OnRiddleComplete(Scenario.Riddle r)
        {
            WeightedRiddle rw = riddles.Where(w => w.riddle == r).First();

            ScenarioProgression += rw.GetWeight();
        }

        private void OnRiddleStart(Scenario.Riddle r)
        {
            WeightedRiddle rw = riddles.Where(w => w.riddle == r).First();

            CurrentRiddleWeight = rw.GetWeight();
            CurrentRiddleProgression = 0.0f;
            CurrentRiddleDuration = 0.0f;

        }

        private void OnElementComplete(Scenario.Element e)
        {
            WeightedRiddle rw = riddles.Where(w => w.riddle.Elements.Contains(e)).First();
            CurrentRiddleProgression += rw.weightedElements.Where(w => w.element == e).First().weight;
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