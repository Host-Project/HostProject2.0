using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SwitchToggle : MonoBehaviour
{
    public Toggle toggle;

    public void Switch()
    {
        GetComponent<Toggle>().isOn = !GetComponent<Toggle>().isOn; 
    }
}
