using FMETP;
using HOST;
using HOST.Monitoring.Settings;
using HOST.Networking;
using HOST.Scenario;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HOST.Monitoring
{


    public class ScenarioManager : MonoBehaviour
    {
        [SerializeField]
        private ScenarioSettings settings;

        public UnityEvent<float> onTimerTick;

        private int currentRiddleIndex = 0;

        private float lastHintTime = 0;
        private float lastPertubatorTime = 0;

        private ProgressionState currentScenarioState;
        private ProgressionState currentRiddleState;


        [SerializeField]
        private ProgressionState scenarioEarlyState = new ScenarioEarlyState();
        [SerializeField]
        private ProgressionState scenarioLateState = new ScenarioLateState();
        [SerializeField]
        private ProgressionState scenarioOnTimeState = new OnTimeState();

        [SerializeField]
        private ProgressionState riddleEarlyState = new RiddleEarlyState();
        [SerializeField]
        private ProgressionState riddleLateState = new RiddleLateState();
        [SerializeField]
        private ProgressionState riddleOnTimeState = new OnTimeState();

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //var tmp = FindObjectsByType<Scenario.Scenario>(FindObjectsSortMode.None);
            settings.Scenario = FindFirstObjectByType<Scenario.Scenario>();
            ResetSettings();
            
            
            settings.Scenario.StartScenario();
        }

        private void ResetSettings()
        {
            currentScenarioState = scenarioOnTimeState;
            currentRiddleState = riddleOnTimeState;

            lastHintTime = -settings.TimeBetweenHints;
            lastPertubatorTime = -settings.TimeBetweenPertubators;

            settings.Scenario.onScenarioStart.AddListener(StartTimer);
            settings.Scenario.onScenarioComplete.AddListener(StopTimer);

            foreach (RiddleSettings rs in settings.Riddles)
            {
                rs.ScenarioSettings = settings;
                rs.Riddle.onRiddleComplete.AddListener(OnRiddleComplete);

                foreach (Element element in rs.Riddle.Elements)
                {
                    element.onComplete.AddListener(OnElementComplete);
                }

            }
        }


        public void LoadScenario()
        {
            SceneManager.LoadScene(settings.Scene.name);
        }
        public void SetScenarioSettings(ScenarioSettings settings)
        {
            this.settings = settings;
        }

        private void OnRiddleComplete(Riddle r)
        {
            currentRiddleIndex++;
        }

        private void OnElementComplete(Element e)
        {
            settings.LastCompletionTime = settings.Time;
        }

        public void StartTimer()
        {
            if (HostNetworkManager.instance.IsServer())
            {
                InvokeRepeating("TimerTick", 0.0f, 1.0f);
            }
        }

        public void StopTimer()
        {
            CancelInvoke("TimerTick");
        }

        private void TimerTick()
        {
            ((SimulationSettings)settings).Tick();
            ((SimulationSettings)settings.GetRiddleSettings(currentRiddleIndex)).Tick();

            ComputeCurrentState();


            int influenceLevel = currentScenarioState.ComputeInfluenceLevel(settings) + currentRiddleState.ComputeInfluenceLevel(settings.GetRiddleSettings(currentRiddleIndex)); 
            if (influenceLevel != 0)
            {

                if (influenceLevel > 0 && settings.Time - lastHintTime >= settings.TimeBetweenHints)
                {
                    settings.Scenario.PlayInfluence(influenceLevel);
                    lastHintTime = settings.Time;
                }
                else if (influenceLevel < 0 && settings.Time - lastPertubatorTime >= settings.TimeBetweenPertubators)
                {
                    settings.Scenario.PlayInfluence(influenceLevel);
                    lastPertubatorTime = settings.Time;
                }
            }

            onTimerTick.Invoke(settings.Time);
        }

        private void ComputeCurrentState()
        {

            RiddleSettings currentRiddleSettings = settings.GetRiddleSettings(currentRiddleIndex);

            float expectedScenarioProgressionAtT = settings.Time / settings.ExpectedDuration();
            float expectedRiddleProgressionAtT = currentRiddleSettings.Time / currentRiddleSettings.ExpectedDuration();

            float deltaRiddleProgression = ((SimulationSettings)currentRiddleSettings).GetProgressionPercentage() - expectedRiddleProgressionAtT;
            float deltaScenarioProgression = ((SimulationSettings)settings).GetProgressionPercentage() - expectedScenarioProgressionAtT;

            if (deltaRiddleProgression > settings.DeltaOnTime)
            {
                currentRiddleState = riddleEarlyState;
            }
            else if (deltaRiddleProgression < -settings.DeltaOnTime)
            {
                currentRiddleState = riddleLateState;
            }
            else
            {
                currentRiddleState = riddleOnTimeState;
            }

            if (deltaScenarioProgression > settings.DeltaOnTime)
            {
                currentScenarioState = scenarioEarlyState;
            }
            else if (deltaScenarioProgression < -settings.DeltaOnTime)
            {
                currentScenarioState = scenarioLateState;
            }
            else
            {
                currentScenarioState = scenarioOnTimeState;
            }

        }

        public float GetTime()
        {
            return settings.Time;
        }



    }
}