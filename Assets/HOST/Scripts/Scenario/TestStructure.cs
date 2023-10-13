using FMETP;
using HOST.Networking;
using HOST.Scenario;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestStructure : MonoBehaviour
{
    public void OnScenarioStart(GameObject obj)
    {
        Debug.Log("Scenario Started, Event received from " + obj.name);

    }

    public void OnScenarioComplete(GameObject obj)
    {
        Debug.Log("Scenario Completed, Event received from " + obj.name);
    }


    public void OnRiddleGlobalStart(GameObject obj)
    {
        Debug.Log("Riddle Global Started, Event received from " + obj.name);
    }

    public void OnRiddleStart(TMP_Text text)
    {
        text.text = "Started";
        text.color = Color.red;

    }

    public void OnRiddleComplete(TMP_Text text)
    {
        text.text = "Done!";
        text.color = Color.green;
    }

    public void OnElementComplete(TMP_Text text)
    {
        text.text = "Done!";
        text.color = Color.green;
    }

    public void RequestComplete(Element e)
    {
        if(FMNetworkManager.instance.NetworkType == FMNetworkType.Client)
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = e.InstanceId,
                MethodName = "Complete",
                Parameters = new object[] { }
            });
        }
        else
        {
            e.Complete();
        }
       
    }

}
