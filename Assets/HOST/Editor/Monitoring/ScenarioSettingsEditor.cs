using HOST.Monitoring;
using HOST.Monitoring.Settings;
using HOST.Scenario;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScenarioSettings))]
public class ScenarioSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UpdateRiddles();

    }

    private void UpdateRiddles()
    {

        ScenarioSettings settings = target as ScenarioSettings;
        settings.Scenario = settings.GetComponent<Scenario>();

        if (AnyRiddleUpdate(settings))
        {
            
            List<RiddleSettings> tmp = settings.Riddles == null ? new List<RiddleSettings>() : new List<RiddleSettings>(settings.Riddles);
            settings.Riddles = new List<RiddleSettings>();
            foreach (Riddle riddle in settings.Scenario.GetRiddles())
            {

                RiddleSettings rs = new RiddleSettings();
                try
                {
                    rs = tmp.Where(x => x.Riddle == riddle).First();

                }
                catch
                {
                    // Nothing to do
                }

                RiddleSettings r = new RiddleSettings()
                {
                    Riddle = riddle,
                    Elements = rs.Elements != null ? new List<WeightedElements>(rs.Elements) : new List<WeightedElements>()
                };

                if(AnyElementsUpdate(r, riddle))
                {
                    UpdateElements(ref r, riddle);
                }

                settings.Riddles.Add(r);
            }
        }

    }

    private void UpdateElements(ref RiddleSettings rs, Riddle r)
    {
        List<WeightedElements> tmp = new List<WeightedElements>(rs.Elements);
        rs.Elements = new List<WeightedElements>();

        foreach (Element e in r.Elements)
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


            rs.Elements.Add(new WeightedElements()
            {
                element = e,
                weight = weight
            });
        }
    }

    private bool AnyRiddleUpdate(ScenarioSettings settings)
    {
        if (settings.Riddles == null || settings.Riddles.Count != settings.Scenario.GetRiddles().Count) return true;

        for (int i = 0; i < settings.Riddles.Count; i++)
        {
            if (settings.Riddles[i] == null || settings.Scenario.GetRiddles()[i] != settings.Riddles[i].Riddle)
            {
                return true;
            }

            if (AnyElementsUpdate(settings.Riddles[i], settings.Scenario.GetRiddles()[i])) return true;
        }

        return false;
    }

    private bool AnyElementsUpdate(RiddleSettings rs, Riddle riddle)
    {
        if (riddle.Elements.Count != rs.Elements.Count) return true;

        for (int i = 0; i < rs.Elements.Count; i++)
        {
            if (riddle.Elements[i] != rs.Elements[i].element) return true;
        }

        return false;
    }
}
