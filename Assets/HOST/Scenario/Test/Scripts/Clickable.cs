using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Clickable : MonoBehaviour
{
    [SerializeField]
    private InputAction mouseClick;

    [SerializeField]
    private UnityEvent onClick;

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
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                onClick.Invoke();
            }
        }
    }
}
