using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ElementsUI : MonoBehaviour
{
    [SerializeField]
    TMP_InputField input;
    [SerializeField]
    TMP_Text elementName;

    public TMP_Text GetElementName()
    {
        return elementName;
    }
    public void SetElementName(string name)
    {
        elementName.text = name;
    }

    public TMP_InputField GetInputField()
    {
        return input;
    }

    public void SetElementInput(string input)
    {
        this.input.text = input;
    }
}
