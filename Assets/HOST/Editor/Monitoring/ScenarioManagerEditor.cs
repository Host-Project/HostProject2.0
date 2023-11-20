using UnityEngine;
using System.Collections;
using UnityEditor;
using HOST.Monitoring;
using HOST.Scenario;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using HOST.Monitoring.Settings;

//[CustomEditor(typeof(ScenarioSettings))]
public class ScenarioManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //UpdateRiddles();

    }

    private void UpdateRiddles()
    {
        ScenarioSettings manager = (ScenarioSettings)target;
      

        /*if (AnyRiddleUpdate(manager))
        {
            List<WeightedRiddle> tmp = new List<WeightedRiddle>(manager.riddles);
            manager.riddles = new List<WeightedRiddle>();
            foreach (Riddle riddle in manager.Scenario.GetRiddles())
            {

                WeightedRiddle wr = new WeightedRiddle();
                try
                {
                    wr = tmp.Where(x => x.riddle == riddle).First();

                }
                catch
                {
                    // Nothing to do
                }

                WeightedRiddle r = new WeightedRiddle()
                {
                    riddle = riddle,
                    weightedElements = wr.weightedElements != null ? new List<WeightedElements>(wr.weightedElements) : new List<WeightedElements>()
                };

                if(AnyElementsUpdate(r, riddle))
                {
                    UpdateElements(ref r, riddle);
                }

                manager.riddles.Add(r);
            }
        }*/
        
    }
/*
    private void UpdateElements(ref WeightedRiddle wr, Riddle r)
    {
        List<WeightedElements> tmp = new List<WeightedElements>(wr.weightedElements);
        wr.weightedElements = new List<WeightedElements>();

        foreach(Element e in r.Elements)
        {
            int weight = 0;
            try
            {
                WeightedElements we = tmp.Where(x => x.element == e).First();
                weight = we.weight;
            }
            catch
            {
                // Nothing to do
            }


            wr.weightedElements.Add(new WeightedElements()
            {
                element = e,
                weight = weight
            });
        }
    }

    private bool AnyRiddleUpdate(ScenarioManager manager)
    {
        if (manager.riddles.Count != manager.Scenario.GetRiddles().Count) return true;

        for(int i = 0; i < manager.riddles.Count; i++)
        {
            if (manager.Scenario.GetRiddles()[i] != manager.riddles[i].riddle) return true;

            if (AnyElementsUpdate(manager.riddles[i], manager.Scenario.GetRiddles()[i])) return true;
        }

        return false;
    }

    private bool AnyElementsUpdate(WeightedRiddle wr, Riddle riddle)
    {
        if(riddle.Elements.Count != wr.weightedElements.Count) return true;

        for(int i = 0; i < wr.weightedElements.Count; i++)
        {
            if (riddle.Elements[i] != wr.weightedElements[i].element) return true;
        }

        return false;
    }
*/
}
