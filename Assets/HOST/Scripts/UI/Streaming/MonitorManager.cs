using DigitalOpus.MB.Core;
using FMETP;
using HOST.Influencers.Interactors;
using HOST.Networking;
using Meta.Voice;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MonitorManager : HostNetworkRPC
{
    [SerializeField]
    private GameObject streamPrefab;

    [SerializeField]
    private Transform mainStream;

    [SerializeField]
    private Transform streamList;

    [SerializeField]
    private GameViewEncoder encoder;

    [SerializeField]
    private TextualInteractor textualInteractor;

    private Dictionary<string, GameObject> streamings = new Dictionary<string, GameObject>();

    private ushort streamingId = 2000;

    public static MonitorManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        base.Start();
        if (HostNetworkManager.instance.IsServer())
        {
            HostNetworkManager.instance.LoadStreams();
            Invoke("InitializeStreams", 2f);
        }

          
    }


    private void OnDataByteReady(byte[] data)
    {
        Debug.Log("Should Sending Stream to server");
        FMNetworkManager.instance.SendToServer(data);
    }


    

    public void RegisterNewStream(string ip)
    {
        if (!HostNetworkManager.instance.IsServer())
            return;
        streamings.Add(ip, null);
    }

    public void UnregisterStream(string ip)
    {
        if (!HostNetworkManager.instance.IsServer())
            return;
        if (streamings.ContainsKey(ip))
        {
            Destroy(streamings[ip]);
            streamings.Remove(ip);
        }
    }

    public void InitializeStreams()
    {
        if (!HostNetworkManager.instance.IsServer())
            return;

        foreach(Transform transform in streamList)
        {
            Destroy(transform.gameObject);
        }
        foreach (string ip in streamings.Keys)
        {
            GameObject stream = Instantiate(streamPrefab, mainStream.childCount == 0 ? mainStream : streamList);
            stream.GetComponent<Button>().onClick.AddListener(() => SwitchMainVideo(stream));
            GameViewDecoder decoder = stream.GetComponent<GameViewDecoder>();
            decoder.label = streamingId;
            FMNetworkManager.instance.OnReceivedByteDataEvent.AddListener(decoder.Action_ProcessImageData);
            SetStreamingId(ip, streamingId);
            streamingId++;
            //streamings[ip] = stream;
        }
    }

    public void SwitchMainVideo(GameObject video)
    {
        // Check if it's not already the main video
        if (video.transform.parent == mainStream.transform)
        {
            return;
        }

        // Otherwise we need to swap
        var child = mainStream.transform.GetChild(0);
        int siblingIndex = video.transform.GetSiblingIndex();



        child.transform.SetParent(video.transform.parent, false);
        video.transform.SetParent(mainStream.transform, false);

        child.transform.SetSiblingIndex(siblingIndex);

        LayoutRebuilder.ForceRebuildLayoutImmediate(child.transform.parent.GetComponent<RectTransform>());

        SetToParentSize(video.GetComponent<RectTransform>());
    }

    private void SetStreamingId(string ip, int id)
    {
        if (HostNetworkManager.instance.IsServer())
        {
            Debug.Log("Setting streaming id");
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "SetStreamingId",
                Parameters = new object[] { ip, id }
            }, ip);
        }
        else
        {
            encoder.label = Convert.ToUInt16(id);
            encoder.OnDataByteReadyEvent.AddListener(OnDataByteReady);
        }
    }

    public void SendMessageToPlayers(string text)
    {
        Debug.Log("Sending message to players");
        if (HostNetworkManager.instance.IsServer())
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "SendMessageToPlayers",
                Parameters = new object[] { text }
            });
        }
        else
        {
            Debug.Log("Showing text");
            textualInteractor.ShowText(text);
        }
    }

    private void SetToParentSize(RectTransform transform)
    {
        // Set the size of the rect transform to the size of the parent
        transform.anchorMin = Vector2.zero;
        transform.anchorMax = Vector2.one;
        transform.sizeDelta = Vector2.zero;
        transform.anchoredPosition = Vector2.zero;
    }
}
