using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlowMove : MonoBehaviour
{
    [SerializeField]
    private InputAction mouseClick;

    [SerializeField]
    GameObject myObject;

    private void OnEnable()
    {
        mouseClick.Enable();
        mouseClick.performed += MousePressed;
    }

    private void OnDisable()
    {
        mouseClick.performed -= MousePressed;
        mouseClick.Disable();
    }

    private void MousePressed(InputAction.CallbackContext context)
    {
        Debug.Log("MousePressed");
        if(FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
        {
            Debug.Log("Server - MousePressed");
            myObject.transform.Translate(Vector3.forward * 0.5f);
        }
        else
        {
            Debug.Log("Client - MousePressed");
            string json = JsonUtility.ToJson(Vector3.forward * 0.5f);
            Debug.Log("json: " + json);
            FMNetworkManager.instance.SendToServer(json);
        }
    }
}
