using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Monitoring
{
    public class ScenarioLateState : ProgressionState
    {
        public override int ComputeInfluenceLevel(ProgressionStateManager manager)
        {
            float expectedProgressionAtT = manager.CurrentTime / manager.ExpectedDuration;
            float deltaProgression = manager.GetScenarioProgressionPercentage() - expectedProgressionAtT;

            if (deltaProgression < -1f / 3f)
            {
                return 2;
            }
            else if (deltaProgression < -0.25f)
            {
                return 1;
            }


            return 0;
        }

    }
}
