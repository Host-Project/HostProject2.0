using HOST.Scenario;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

namespace HOST.Monitoring.Settings
{

    [RequireComponent(typeof(Scenario.Scenario))]
    public class ScenarioSettings : MonoBehaviour, SimulationSettings
    {
        [SerializeField]
        private Scenario.Scenario scenario;

        [SerializeField]
        private SceneAsset scene;

        [SerializeField]
        private float expectedDuration = 600f;

        [SerializeField]
        private float timeBetweenHints = 30f;

        [SerializeField]
        private float timeBetweenPertubators = 15f;

        [SerializeField]
        private float deltaOnTime = 0.1f;

        [SerializeField]
        private List<RiddleSettings> riddles; // Automatically updated by the editor


        public Scenario.Scenario Scenario { get => scenario; set => scenario = value; }
        public float TimeBetweenHints { get => timeBetweenHints; set => timeBetweenHints = value; }
        public float TimeBetweenPertubators { get => timeBetweenPertubators; set => timeBetweenPertubators = value; }
        public float DeltaOnTime { get => deltaOnTime; set => deltaOnTime = value; }
        public List<RiddleSettings> Riddles { get => riddles; set => riddles = value; }
        public float LastCompletionTime { get; set;}
        public float Time { get; set; }
        public SceneAsset Scene { get => scene; set => scene = value; }

        private void Start()
        {
            foreach(RiddleSettings rs in Riddles)
            {
                rs.ScenarioSettings = this;
            }
        }

        public int GetWeight()
        {
            return Riddles.Sum(r => r.GetWeight());
        }

        public float GetProgression()
        {
            return Riddles.Sum(r => r.GetProgression());
        }

        public RiddleSettings GetRiddleSettings(int index)
        {
            return Riddles[index];
        }


        public void SetExpectedDuration(float duration)
        {
            expectedDuration = duration;
        }
        public float ExpectedDuration()
        {
            return expectedDuration;
        }
    }

}
