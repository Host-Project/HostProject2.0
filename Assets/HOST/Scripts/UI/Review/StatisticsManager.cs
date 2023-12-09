using HOST.Monitoring;
using HOST.Monitoring.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text durationText;

    [SerializeField]
    private TMP_Text expectedDurationText;

    [SerializeField]
    private GameObject riddlesList;


    [SerializeField]
    private GameObject riddlePrefab;


    private ScenarioManager scenarioManager;

    void Start()
    {
        scenarioManager = ScenarioManager.instance;
        UpdateUI();

    }

    private void UpdateUI()
    {
        durationText.text = string.Format("{0} minutes", Convert.ToInt32(scenarioManager.settings.Time / 60f));
        expectedDurationText.text = string.Format("{0} minutes", Convert.ToInt32(scenarioManager.settings.ExpectedDuration() / 60f));

        //Destroy(riddlesList.transform.GetChild(0));

        foreach (RiddleSettings riddle in scenarioManager.settings.Riddles)
        {
            var riddleObject = Instantiate(riddlePrefab, riddlesList.transform);
            RiddleUI ui = riddleObject.GetComponent<RiddleUI>();
            ui.RiddleName.text = riddle.Name;
            ui.Duration.text = string.Format("{0} minutes", Convert.ToInt32(riddle.Time / 60f));
            ui.ExpectedDuration.text = string.Format("{0} minutes", Convert.ToInt32(riddle.ExpectedDuration() / 60f));
        }
    }
}
