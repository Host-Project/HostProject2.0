using UnityEngine;

namespace HOST.Monitoring
{
    public class ProgressionStateManager : MonoBehaviour
    {
        #region Defined at the beginning of the game

        [SerializeField]
        private float deltaOnTime = 0.1f;

        [SerializeField]
        private float expectedDuration = 0.0f;
        public float ExpectedDuration { get => expectedDuration; set => expectedDuration = value; }


        #endregion

        #region Computed variables

        private float currentTime = 0.0f;
        public float CurrentTime { get => currentTime; set => currentTime = value; }


        private int scenarioWeights = 0;
        public int ScenarioWeights { get => scenarioWeights; set => scenarioWeights = value; }


        private float scenarioProgression = 0.0f;
        public float ScenarioProgression { get => scenarioProgression; set => scenarioProgression = value; }


        private int currentRiddleWeight = 0;
        public int CurrentRiddleWeight { get => currentRiddleWeight; set => currentRiddleWeight = value; }


        private float currentRiddleDuration = 0.0f;
        public float CurrentRiddleDuration { get => currentRiddleDuration; set => currentRiddleDuration = value; }


        private float currentRiddleProgression = 0.0f;
        public float CurrentRiddleProgression { get => currentRiddleProgression; set => currentRiddleProgression = value; }


        private float lastCompletionTime = 0.0f;
        public float LastCompletionTime { get => lastCompletionTime; set => lastCompletionTime = value; }


        #endregion

        public ProgressionState currentScenarioState;
        public ProgressionState currentRiddleState;



        private void Awake()
        {
            currentScenarioState = new OnTimeState();
            currentRiddleState = new OnTimeState();
        }


        public virtual int ComputeInfluenceLevel()
        {
            ComputeCurrentState();
            return currentScenarioState.ComputeInfluenceLevel(this) + currentRiddleState.ComputeInfluenceLevel(this);
        }


        public float CurrentRiddleExpectedDuration()
        {
            return ExpectedDuration / ScenarioWeights * CurrentRiddleWeight;
        }

        public float DurationSinceLastCompletion()
        {
            return CurrentTime - LastCompletionTime;
        }

        private void ComputeCurrentState()
        {

            float expectedScenarioProgressionAtT = CurrentTime / ExpectedDuration;
            float expectedRiddleProgressionAtT = CurrentRiddleDuration / CurrentRiddleExpectedDuration();

            float deltaRiddleProgression = GetCurrentRiddleProgressionPercentage() - expectedRiddleProgressionAtT;
            float deltaScenarioProgression = GetScenarioProgressionPercentage() - expectedScenarioProgressionAtT;

            if (deltaRiddleProgression > deltaOnTime)
            {
                currentRiddleState = new RiddleEarlyState();
            }
            else if (deltaRiddleProgression < -deltaOnTime)
            {
                currentRiddleState = new RiddleLateState();
            }
            else
            {
                currentRiddleState = new OnTimeState();
            }

            if (deltaScenarioProgression > deltaOnTime)
            {
                currentScenarioState = new ScenarioEarlyState();
            }
            else if (deltaScenarioProgression < -deltaOnTime)
            {
                currentScenarioState = new ScenarioLateState();
            }
            else
            {
                currentScenarioState = new OnTimeState();
            }

        }


        public float GetScenarioProgressionPercentage()
        {
            return ScenarioProgression / ScenarioWeights;
        }

        public float GetCurrentRiddleProgressionPercentage()
        {
            return CurrentRiddleProgression / CurrentRiddleWeight;
        }
    }
}