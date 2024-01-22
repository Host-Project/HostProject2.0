using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using HOST.Networking;

public class SimulationStarterManager : MonoBehaviour
{
    [SerializeField]
    private Color color1;
    [SerializeField]
    private string text1;
    [SerializeField]
    private Color color2;
    [SerializeField]
    private string text2;

    [SerializeField]
    private TMP_Text text;

    [SerializeField]
    private  Image image;

    [SerializeField]
    private UnityEvent OnStart;
    [SerializeField]
    private UnityEvent OnStop;

    private void Start()
    {
        text.text = text1;
        image.color = color1;
    }



    public void Switch()
    {
        if (text.text == text1)
        {
            text.text = text2;
            image.color = color2;
            HostNetworkManager.instance.RequestRegisterNetworkObjects();
            OnStart.Invoke();
        }
        else
        {
            text.text = text1;
            image.color = color1;
            OnStop.Invoke();
        }
    }   
}
