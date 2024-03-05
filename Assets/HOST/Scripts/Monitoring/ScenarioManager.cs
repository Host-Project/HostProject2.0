using FMETP;
using HOST;
using HOST.Monitoring.Settings;
using HOST.Networking;
using HOST.Scenario;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HOST.Monitoring
{


    public class ScenarioManager : MonoBehaviour
    {
        [SerializeField]
        public ScenarioSettings settings;

        public UnityEvent<float> onTimerTick;

        public UnityEvent onRiddleStart;

        private int currentRiddleIndex = 0;

        private float lastHintTime = 0;
        private float lastPertubatorTime = 0;

        public static ScenarioManager instance;


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
            if (instance == null)
            {
                instance = this;
            }
        }

     
        public  void StartScenario()
        {
            settings.Scenario = FindFirstObjectByType<Scenario.Scenario>();
            ResetSettings();
            settings.Scenario.StartScenario();
            onRiddleStart.Invoke();
        }

        private void ResetSettings()
        {
            currentScenarioState = scenarioOnTimeState;
            currentRiddleState = riddleOnTimeState;

            lastHintTime = -settings.TimeBetweenHints;
            lastPertubatorTime = -settings.TimeBetweenPertubators;

            
            settings.Scenario.onScenarioStart.AddListener(StartTimer);
            settings.Scenario.onScenarioComplete.AddListener(StopTimer);
            settings.Time = 0;

            
            foreach(var (rs, index) in settings.Riddles.Select((r, i) => (r, i)))
            {
                settings.Riddles[index].Riddle = settings.Scenario.GetRiddles()[index];
                rs.Time = 0;
                rs.ScenarioSettings = settings;
                rs.Name = rs.Riddle.name;
                rs.Riddle.onRiddleComplete.AddListener(OnRiddleComplete);

                foreach (var (element, i) in rs.Elements.Select((e, i) => (e,i)))
                {
                    element.element = rs.Riddle.Elements[i];
                    element.element.onComplete.AddListener(OnElementComplete);
                }

            }
            
        }

        public void EndScenario()
        {
            SceneLoader.instance.RequestLoadScene("03_Review");
        }

        public void LoadScenario()
        {
            SceneLoader.instance.RequestLoadScene(settings.SceneName);
        }
        public void SetScenarioSettings(ScenarioSettings settings)
        {
            this.settings = settings;
        }

        private void OnRiddleComplete(Riddle r)
        {
            currentRiddleIndex++;
            currentRiddleIndex = Mathf.Min(currentRiddleIndex, settings.Riddles.Count - 1);
            onRiddleStart.Invoke();
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


            int influenceLevel = currentScenarioState.ComputeInfluenceLevel(settings) + 
                                 currentRiddleState.ComputeInfluenceLevel(settings.GetRiddleSettings(currentRiddleIndex));

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

          /*  Debug.Log("Scenario " + expectedScenarioProgressionAtT);
            Debug.Log("Riddle " + expectedRiddleProgressionAtT);
            Debug.Log("Delta r " + deltaRiddleProgression);
            Debug.Log("Delta s " + deltaScenarioProgression);
            Debug.Log("S prog " + ((SimulationSettings)settings).GetProgressionPercentage());
            Debug.Log("----");*/

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

        public void CompleteCurrentRiddle()
        {
            settings.Scenario.CompleteCurrentRiddle();
        }
        public float GetTime()
        {
            return settings.Time;
        }

        public float GetCurrentRiddleTime()
        {
            return settings.GetRiddleSettings(currentRiddleIndex).Time;
        }

        public string GetCurrentRiddleName()
        {
            return settings.GetRiddleSettings(currentRiddleIndex).Name;
        }

        public float GetCurrentRiddleExpectedTime()
        {
            return settings.GetRiddleSettings(currentRiddleIndex).ExpectedDuration();
        }

      
        public float GetExpectedTime()
        {
            return settings.ExpectedDuration();
        }
        public ProgressionState.StateType GetCurrentScenarioStateType()
        {
            return currentScenarioState.GetStateType();
        }

        public ProgressionState.StateType GetCurrentRiddleStateType()
        {
            return currentRiddleState.GetStateType();
        }

    }
}