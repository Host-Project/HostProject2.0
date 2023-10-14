using HOST.Scenario;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace HOST.Monitoring
{
    [Serializable]
    public struct WeightedRiddle
    {
        public Riddle riddle;
        public int weight;
    }
    public class ScenarioManager : MonoBehaviour
    {

        public List<WeightedRiddle> riddles;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}