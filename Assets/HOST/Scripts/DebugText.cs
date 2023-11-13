using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    public void Debug(string txt)
    {
        text.text = txt;
        CancelInvoke("EmptyText");
        Invoke("EmptyText", 5f);
    }

    public void EmptyText()
    {
        text.text = string.Empty;
    }
}
