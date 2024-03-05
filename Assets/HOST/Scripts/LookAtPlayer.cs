using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LookAtPlayer : MonoBehaviour
{
    OVRCameraRig cameraRig;
    private static bool rotate = true;
    // Start is called before the first frame update
    void Start()
    {
        cameraRig = FindAnyObjectByType<OVRCameraRig>();
    }

    // Update is called once per frame
    void Update()
    {
        cameraRig = FindAnyObjectByType<OVRCameraRig>();
        if (cameraRig != null && rotate)
        {
            Vector3 targetPostition = new Vector3(cameraRig.centerEyeAnchor.position.x, this.transform.position.y, cameraRig.centerEyeAnchor.position.z);
            this.transform.LookAt(targetPostition);
        }
    }

    public void Rotate()
    {
        rotate = true;
    }

    public void StopRotate()
    {
        rotate = false;
    }
}

   
