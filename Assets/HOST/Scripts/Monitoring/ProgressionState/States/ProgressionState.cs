using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Monitoring
{
    public abstract class ProgressionState
    {
        protected const int SCENARIO_MAX_INFLUENCE = 2;
        protected const int RIDDLE_MAX_INFLUENCE = 5;
        public abstract int ComputeInfluenceLevel(ProgressionStateManager manager);

    }
}