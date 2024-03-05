//using HostProject.Network;
using FMETP;
using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class MonitoringRidle : HostNetworkRPC
{
    public UnityEvent OnSuccess;
    public UnityEvent OnMaxStreak;

    [SerializeField]
    private Light success;

    [SerializeField]
    private Light error;

    [SerializeField]
    private List<Light> lights;

    // [SerializeField]
    // private HelpRPC rpc;

    [SerializeField]
    private GameObject backgroundMonitoringHint;

    [SerializeField]
    private Image displayedImage;

    [SerializeField]
    private List<Sprite> arrows;

    [SerializeField]
    private Sprite periodicTable;

    [SerializeField]
    private GameObject heartrateMonitor;

    public AudioSource monitorSound;

    private int lastId = -1;

    private bool showImage = false;
    private float timeLeft = 0.0f;
    private int imageIndex = 0;



    // Update is called once per frame
    void Update()
    {
        timeLeft += Time.deltaTime;
        if (showImage && timeLeft >= 1.0f)
        {
            int indexMax = counter / step > arrows.Count ? arrows.Count + 1 : counter / step;
            if (indexMax > imageIndex)
            {
                ShowImage(imageIndex);
                imageIndex++;
            }
            else
            {
                HideImage();
                showImage = false;
                imageIndex = 0;
            }
            timeLeft = 0.0f;
        }
    }

    private void HideImage()
    {
        backgroundMonitoringHint.SetActive(false);
        displayedImage.gameObject.SetActive(false);
    }

    private void ShowImage(int id)
    {
        backgroundMonitoringHint.SetActive(true);
        displayedImage.sprite = id == arrows.Count ? periodicTable : arrows[id];
        if (id > arrows.Count)
        {
            OnMaxStreak.Invoke();
        }

        displayedImage.gameObject.SetActive(true);


    }
    public void SendPushedButton(int id)
    {
        if (FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
        {

            ButtonClicked(id);
        }
        else
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "ButtonClicked",
                Parameters = new object[] { id }
            });
        }
    }

    private int counter = 0;

    public int step;

    private void IsSuccess(bool success)
    {
        this.success.gameObject.SetActive(false);
        this.error.gameObject.SetActive(false);

        if (lastId != -1)
        {
            lights[lastId - 1].gameObject.SetActive(false);
        }

        if (success)
        {
            OnSuccess.Invoke();
            counter++;
            showImage = true;
            this.success.gameObject.SetActive(true);
            StartCoroutine(StopLight(this.success, 2));

        }
        else
        {
            if (counter > 0)
                counter--;
            this.error.gameObject.SetActive(true);
            StartCoroutine(StopLight(this.error, 2));
        }
    }

    public void ActiveButton(int id)
    {

        if (lastId != -1)
        {
            lights[lastId - 1].gameObject.SetActive(false);
            lights[id - 1].gameObject.SetActive(true);
        }

        lastId = id;
    }

    private bool hasBeenPressed = false;
    private int currentButton = -1;

    public void SendMonitoring()
    {
        if (FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
        {
            if (!hasBeenPressed)
            {
                Feedback(false);
            }
            currentButton = Random.Range(1, 6);
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "ActiveButton",
                Parameters = new object[] { currentButton }
            });

            ActiveButton(currentButton);

        }
        Invoke("SendMonitoring", 10);
    }

    private void ButtonClicked(int id)
    {
        hasBeenPressed = true;
        Feedback(id == currentButton);
        currentButton = -1;
    }
    private void Feedback(bool success)
    {
        if (FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "IsSuccess",
                Parameters = new object[] { success }
            });

            IsSuccess(success);
        }
    }

    public void OnEnd()
    {
        CancelInvoke("SendMonitoring");
    }

    IEnumerator StopLight(Light l, int time)
    {
        yield return new WaitForSeconds(time);

        l.gameObject.SetActive(false);
    }


}
