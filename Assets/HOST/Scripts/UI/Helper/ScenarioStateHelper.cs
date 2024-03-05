using HOST.Monitoring;
using HOST.Debriefing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HOST.Networking;
using TMPro;
using System;

public class ScenarioStateHelper : MonoBehaviour
{
    private ScenarioManager scenarioManager;
    private DebriefingManager videoManager;

    public static ScenarioStateHelper instance;


    [SerializeField]
    TMP_Text riddleTimeText;
    [SerializeField]
    TMP_Text scenarioTimeText;
    [SerializeField]
    TMP_Text riddleNameText;




    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        scenarioManager = FindAnyObjectByType<ScenarioManager>();
        if (scenarioManager == null)
        {
            Debug.LogError("ScenarioManager not found");
            return;
        }

        videoManager = FindAnyObjectByType<DebriefingManager>();
        if (videoManager == null)
        {
            Debug.LogError("VideoManager not found");
            return;
        }

        scenarioManager.onTimerTick.AddListener(OnTimerTick);
        scenarioManager.onRiddleStart.AddListener(OnRiddleStart);
        riddleNameText.text = scenarioManager.GetCurrentRiddleName();
    }

    private void OnRiddleStart()
    {
        riddleNameText.text = scenarioManager.GetCurrentRiddleName();
    }

    private void OnTimerTick(float time)
    {
        SetTimerText(scenarioTimeText, TimeSpan.FromSeconds(scenarioManager.GetTime()), TimeSpan.FromSeconds(scenarioManager.GetExpectedTime()), scenarioManager.GetCurrentScenarioStateType());
        SetTimerText(riddleTimeText, TimeSpan.FromSeconds(scenarioManager.GetCurrentRiddleTime()), TimeSpan.FromSeconds(scenarioManager.GetCurrentRiddleExpectedTime()), scenarioManager.GetCurrentRiddleStateType());
    }

    private void SetTimerText(TMP_Text text, TimeSpan time, TimeSpan expected, ProgressionState.StateType type)
    {
        switch (type)
        {
            case ProgressionState.StateType.EARLY:
                text.color = Color.green;
                break;
            case ProgressionState.StateType.LATE:
                text.color = Color.red;
                break;
            case ProgressionState.StateType.ON_TIME:
                text.color = Color.black;
                break;
        }

        text.text = string.Format("{0:D2}:{1:D2}:{2:D2} / {3:D2}:{4:D2}:{5:D2}",
                       time.Hours, time.Minutes, time.Seconds, expected.Hours, expected.Minutes, expected.Seconds);
    }

    public void EndScenario()
    {
        videoManager.StopRecordingVideo();
        Invoke("WaitToChangeScene", 10f);
    }

    private void WaitToChangeScene()
    {
        scenarioManager.EndScenario();
    }

    public void StartScenario()
    {
        HostNetworkManager.instance.RequestRegisterNetworkObjects();
        scenarioManager.StartScenario();

    }

    public void CompleteCurrentRiddle()
    {
        scenarioManager.CompleteCurrentRiddle();
    }

    


}
