using HOST.Monitoring.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Monitoring
{
    public class OnTimeState : ProgressionState
    {
        public override int ComputeInfluenceLevel(SimulationSettings manager)
        {
            return 0;
        }

        public override StateType GetStateType()
        {
            return StateType.ON_TIME;
        }
    }
}
