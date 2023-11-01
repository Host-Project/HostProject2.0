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

        public List<WeightedRiddle> riddles; // Automatically updated by the editor

        public UnityEvent<float> onTimerTick;

        [SerializeField]
        private float timeBetweenHints = 30f;
        
        [SerializeField]
        private float timeBetweenPertubators = 15f;

        private float lastHintTime;
        private float lastPertubatorTime;

        private void Start()
        {
            lastHintTime = -timeBetweenHints;
            lastPertubatorTime = -timeBetweenPertubators;

            Scenario.Scenario.instance.onScenarioStart.AddListener(StartTimer);
            Scenario.Scenario.instance.onScenarioComplete.AddListener(StopTimer);
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
                
                if (influenceLevel > 0 && CurrentTime-lastHintTime >= timeBetweenHints)
                {
                    Scenario.Scenario.instance.PlayInfluence(influenceLevel);
                    lastHintTime = CurrentTime;
                }
                else if (influenceLevel < 0 && CurrentTime-lastPertubatorTime >= timeBetweenPertubators)
                {
                    Scenario.Scenario.instance.PlayInfluence(influenceLevel);
                    lastPertubatorTime = CurrentTime;
                }
            }

            onTimerTick.Invoke(CurrentTime);
        }

        public float GetTime()
        {
            return CurrentTime;
        }



    }
}