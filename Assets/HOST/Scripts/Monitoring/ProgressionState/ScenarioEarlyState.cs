using HOST.Monitoring.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Monitoring
{
    public class ScenarioEarlyState : ProgressionState
    {
        public override int ComputeInfluenceLevel(SimulationSettings settings)
        {
            float deltaProgression = DeltaProgression(settings);

            if (deltaProgression > 1f/3f)
            {
                return -2;
            }
            else if (deltaProgression > 0.25f)
            {
                return -1;
            }


            return 0;
        }
    }
}