using HOST.Monitoring.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Monitoring
{
    public class RiddleEarlyState : ProgressionState
    {
        public override int ComputeInfluenceLevel(SimulationSettings settings)
        {
            float deltaRiddleProgression = DeltaProgression(settings);

            if (deltaRiddleProgression > 0.8f)
            {
                return -5;
            }
            else if (deltaRiddleProgression > 2f / 3f)
            {
                return -4;

            }
            else if (deltaRiddleProgression > 0.5f)
            {
                return -3;
            }
            else if (deltaRiddleProgression > 1f / 3f)
            {
                return -2;
            }
            else if (deltaRiddleProgression > 0.25f)
            {
                return -1;
            }

            return 0;
        }

    }
}