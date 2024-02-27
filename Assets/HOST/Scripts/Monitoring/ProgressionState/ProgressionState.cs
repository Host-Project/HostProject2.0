using HOST.Monitoring.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Monitoring
{
    public abstract class ProgressionState 
    {
        public enum StateType
        {
            LATE,
            ON_TIME,
            EARLY
        }
        public abstract int ComputeInfluenceLevel(SimulationSettings settings);

        public abstract StateType GetStateType();

        public float ExpectedProgression(SimulationSettings settings)
        {
            return settings.Time / settings.ExpectedDuration();
        }

        public float DeltaProgression(SimulationSettings settings)
        {
            return settings.GetProgression() - ExpectedProgression(settings);
        }
    }
}