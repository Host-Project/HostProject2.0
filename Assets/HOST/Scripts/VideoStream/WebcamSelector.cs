using Michsky.MUIP;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebcamSelector : MonoBehaviour
{
    private WebCamDevice[] devices;
    private WebCamTexture[] webCams;
    private Texture[] textures;

    public int TargetCamID = 0;


    public CustomDropdown SelectionDropdown;

    private WebCamTexture webCamTexture;
    public WebCamTexture WebCamTexture { get { return webCamTexture; } }

    public GameObject targetMeshObject;

    private int timeoutFrameCount = 300;

    public Vector2 requestResolution = new Vector2(1280, 720);
    public Vector2 textureResolution;

    private Coroutine RunningWebcamCoroutine;

    private Quaternion baseRotation;

    private bool isFlipped;
    public bool IsFlipped { get { return isFlipped; } }

    // Start is called before the first frame update
    void Start()
    {
        GetAllWebcams();
        RunningWebcamCoroutine = StartCoroutine(InitAndWaitForWebCamTexture(TargetCamID));
    }

    public void OnApplicationQuit() { Action_StopWebcam(); }

    public void Action_StopWebcam()
    {
        //if (!enabled || !gameObject.activeSelf) return;
        if (webCams != null)
        {
            for (int i = 0; i < webCams.Length; i++)
            {
                if (webCams[i].isPlaying) webCams[i].Stop();
                Destroy(webCams[i]);
                Destroy(textures[i]);
            }
        }
        webCams = null;
        devices = null;
        textures = null;

        Destroy(webCamTexture);
        webCamTexture = null;
    }

    private void OnDestroy()
    {
        Action_StopWebcam();
    }

    public void StopWebcamStream()
    {
        StopCoroutine(RunningWebcamCoroutine);
    }

    public void StartWebcamStream(int index)
    {
        StopWebcamStream();
        RunningWebcamCoroutine = StartCoroutine(InitAndWaitForWebCamTexture(index));
    }

    private void GetAllWebcams()
    {
        devices = WebCamTexture.devices;
        webCams = new WebCamTexture[devices.Length];
        textures = new Texture[devices.Length];

        SelectionDropdown.items.Clear();
        for (int i = 0; i < devices.Length; i++)
        {
            //SelectionDropdown.items.Add(new TMPro.TMP_Dropdown.OptionData("[" + i + "] name: " + devices[i].name));
            SelectionDropdown.items.Add(new CustomDropdown.Item()
            {
                itemName = devices[i].name,
                itemIndex = i
            });
            webCams[i] = new WebCamTexture();
            webCams[i] = new WebCamTexture(devices[i].name, Mathf.RoundToInt(requestResolution.x), Mathf.RoundToInt(requestResolution.y), 30);
            textures[i] = webCams[i];
            textures[i].wrapMode = TextureWrapMode.Repeat;
        }


        TargetCamID = 0;
        SelectionDropdown.SetDropdownIndex(TargetCamID);
        SelectionDropdown.onValueChanged.RemoveAllListeners();
        SelectionDropdown.onValueChanged.AddListener((x) => StartWebcamStream(x));
        SelectionDropdown.SetupDropdown();
    }

    IEnumerator InitAndWaitForWebCamTexture(int target)
    {
        TargetCamID = target;

        bool hasCrashed = false;

        try
        {
            //texture = webCams[TargetCamID];
            webCams[TargetCamID].requestedFPS = 30;
            webCams[TargetCamID].Play();
        }
        catch (Exception)
        {
            // Some webcams cannot be opened
            hasCrashed = true;
        }

        // Can't yield return inside exception
        if (hasCrashed)
        {
            yield return null;
        }

        if (targetMeshObject.GetComponent<RawImage>() != null) targetMeshObject.GetComponent<RawImage>().texture = textures[TargetCamID];

        if (textures.Length > 0)
        {
            webCamTexture = webCams[TargetCamID];
            int initFrameCount = 0;

            while (webCamTexture.width <= 16)
            {
                if (initFrameCount > timeoutFrameCount) break;

                initFrameCount++;
                yield return new WaitForEndOfFrame();
            }

            textureResolution = new Vector2(webCamTexture.width, webCamTexture.height);
            isFlipped = webCamTexture.videoVerticallyMirrored;
        }
        yield return null;
    }
}
