using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace HOST.Monitoring.Settings
{
    
    public interface SimulationSettings 
    {


        public float LastCompletionTime { get; set;}
        public float Time { get; set; }


        public float ExpectedDuration();
        public int GetWeight();

        public float GetProgression();

        public float GetProgressionPercentage()
        {
            return GetProgression() / GetWeight();
        }
        public void Tick()
        {
            Time++;
        }
     
        public float DurationSinceLastCompletion()
        {
            return Time - LastCompletionTime;
        }



     
    }
}