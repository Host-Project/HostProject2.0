using UnityEngine;
using System.Collections;
using UnityEditor;
using HOST.Monitoring;
using HOST.Scenario;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Serialization;
using System.Linq;
using static Unity.Burst.Intrinsics.X86.Avx;

[CustomEditor(typeof(ScenarioManager))]
public class ScenarioManagerEditor : Editor
{
    void OnEnable()
    {
        
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UpdateRiddles();

    }

    private void UpdateRiddles()
    {
        ScenarioManager manager = (ScenarioManager)target;
        Scenario scenario = FindObjectsByType<Scenario>(FindObjectsSortMode.None)[0];
      

        if (AnyUpdate(manager, scenario))
        {
            List<WeightedRiddle> tmp = new List<WeightedRiddle>(manager.riddles);
            manager.riddles = new List<WeightedRiddle>();
            foreach (Riddle riddle in scenario.GetRiddles())
            {
                int weight = 0;
                try
                {
                    WeightedRiddle wr = tmp.Where(x => x.riddle == riddle).First();
                    weight = wr.weight;
                }
                catch
                {
                    // Nothing to do
                }

                manager.riddles.Add(new WeightedRiddle()
                {
                    weight = weight,
                    riddle = riddle
                });
            }
        }
        
        
    }

    private bool AnyUpdate(ScenarioManager manager, Scenario scenario)
    {
        if (manager.riddles.Count != scenario.GetRiddles().Count) return true;

        for(int i = 0; i < manager.riddles.Count; i++)
        {
            if (scenario.GetRiddles()[i] != manager.riddles[i].riddle) return true;
        }

        return false;
    }
}
