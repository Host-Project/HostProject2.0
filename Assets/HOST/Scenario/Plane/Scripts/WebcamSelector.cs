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

    [TextArea(2, 10)]
    public string DeviceInfo;

    private WebCamTexture webCamTexture;
    public WebCamTexture WebCamTexture { get { return webCamTexture; } }

    public GameObject targetMeshObject;

    public GameObject BackgroundQuad;

    private int timeoutFrameCount = 300;

    public Vector2 requestResolution = new Vector2(1280, 720);
    public Vector2 textureResolution;

    private float TextureRatio = 1f;
    private float ScreenRatio = 1f;

    private Coroutine RunningWebcamCoroutine;

    private Quaternion baseRotation;

    private bool isFlipped;
    public bool IsFlipped { get { return isFlipped; } }

    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR // We only want to start the camera outside the editor
        GetAllWebcams();
        RunningWebcamCoroutine = StartCoroutine(InitAndWaitForWebCamTexture(TargetCamID));
        if (BackgroundQuad != null) baseRotation = BackgroundQuad.transform.localRotation;
#endif
    }

    public void Update()
    {
#if !UNITY_EDITOR // We only want to start the camera outside the editor
        if (BackgroundQuad != null && webCamTexture != null) CalculateBackgroundQuad();
#endif
    }


    void CalculateBackgroundQuad()
    {
        Camera cam = Camera.main;
        ScreenRatio = (float)Screen.width / (float)Screen.height;

        BackgroundQuad.transform.SetParent(cam.transform);
        BackgroundQuad.transform.localPosition = new Vector3(0f, 0f, cam.farClipPlane / 2f);

        float videoRotationAngle = webCamTexture.videoRotationAngle;

        BackgroundQuad.transform.localRotation = baseRotation * Quaternion.AngleAxis(webCamTexture.videoRotationAngle, Vector3.forward);

        float distance = cam.farClipPlane / 2f;
        float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        BackgroundQuad.transform.localPosition = new Vector3(0f, 0f, distance);
        Vector3 QuadScale = new Vector3(1f, frustumHeight, 1f);

        //adjust the scaling for portrait Mode & Landscape Mode
        if (videoRotationAngle == 0 || videoRotationAngle == 180)
        {
            //landscape mode
            TextureRatio = (float)(webCamTexture.width) / (float)(webCamTexture.height);
            if (ScreenRatio > TextureRatio)
            {
                float SH = ScreenRatio / TextureRatio;
                float TW = TextureRatio * frustumHeight * SH;
                float TH = frustumHeight * (webCamTexture.videoVerticallyMirrored ? -1 : 1) * SH;
                QuadScale = new Vector3(TW, TH, 1f);
            }
            else
            {
                float TW = TextureRatio * frustumHeight;
                QuadScale = new Vector3(TW, frustumHeight * (webCamTexture.videoVerticallyMirrored ? -1 : 1), 1f);
            }
        }
        else
        {
            //portrait mode
            TextureRatio = (float)(webCamTexture.height) / (float)(webCamTexture.width);
            if (ScreenRatio > TextureRatio)
            {
                float SH = ScreenRatio / TextureRatio;
                float TW = frustumHeight * -1f * SH;
                float TH = TW * (webCamTexture.videoVerticallyMirrored ? 1 : -1) * SH;
                QuadScale = new Vector3(TW, TH, 1f);
            }
            else
            {
                float TW = TextureRatio * frustumHeight;
                QuadScale = new Vector3(frustumHeight * -1f, TW * (webCamTexture.videoVerticallyMirrored ? 1 : -1), 1f);
            }
        }
        BackgroundQuad.transform.localScale = QuadScale;
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
#if !UNITY_EDITOR // We only want to start the camera outside the editor
        StopWebcamStream();
        RunningWebcamCoroutine = StartCoroutine(InitAndWaitForWebCamTexture(index));
#endif
    }

    private void GetAllWebcams()
    {
        devices = WebCamTexture.devices;
        webCams = new WebCamTexture[devices.Length];
        textures = new Texture[devices.Length];

        DeviceInfo = "";
        for (int i = 0; i < devices.Length; i++)
        {
            DeviceInfo += "[" + i + "] name: " + devices[i].name + "\n";
            webCams[i] = new WebCamTexture();
            webCams[i] = new WebCamTexture(devices[i].name, Mathf.RoundToInt(requestResolution.x), Mathf.RoundToInt(requestResolution.y), 30);
            textures[i] = webCams[i];
            textures[i].wrapMode = TextureWrapMode.Repeat;
        }

        TargetCamID = 0;
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

        if (targetMeshObject != null && targetMeshObject.GetComponent<RawImage>() != null) targetMeshObject.GetComponent<RawImage>().texture = textures[TargetCamID];
        if (BackgroundQuad != null) BackgroundQuad.GetComponent<Renderer>().material.mainTexture = textures[TargetCamID];

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
