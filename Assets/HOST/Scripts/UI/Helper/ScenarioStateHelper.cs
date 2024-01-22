using HOST.Monitoring;
using HOST.Debriefing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HOST.Networking;

public class ScenarioStateHelper : MonoBehaviour
{
    private ScenarioManager scenarioManager;
    private DebriefingManager videoManager;

    public static ScenarioStateHelper instance;

    void Start()
    {
        if(instance == null)
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
}
