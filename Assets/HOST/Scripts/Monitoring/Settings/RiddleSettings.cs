using HOST.Scenario;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HOST.Monitoring.Settings
{
    [Serializable]
    public class WeightedElements
    {
        public Element element;
        public int weight;
    }

    [Serializable]
    public class RiddleSettings : SimulationSettings
    {

        [SerializeField]
        private Riddle riddle;

        [SerializeField]
        private List<WeightedElements> elements;

        private ScenarioSettings scenarioSettings;

        public Riddle Riddle { get => riddle; set => riddle = value; }
        public List<WeightedElements> Elements { get => elements; set => elements = value; }
        public ScenarioSettings ScenarioSettings { get => scenarioSettings; set => scenarioSettings = value; }
        public float LastCompletionTime { get; set; }
        public float Time { get; set; }

        public float GetProgression()
        {
            return Elements.Sum(e => e.element.IsCompleted() ? e.weight : 0);
        }

        public int GetWeight()
        {
            return Elements.Sum(e => e.weight);
        }

        public float ExpectedDuration()
        {
            return scenarioSettings.ExpectedDuration() * GetWeight() / scenarioSettings.GetWeight();

        }

    }
}