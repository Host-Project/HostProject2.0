using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Monitoring
{
    public class OnTimeState : ProgressionState
    {
        public override int ComputeInfluenceLevel(ProgressionStateManager manager)
        {
            return 0;
        }
    }
}
