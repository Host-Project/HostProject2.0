using TMPro;
using UnityEngine;

public class RuntimeLogs : MonoBehaviour
{
    [SerializeField]
    private TMP_Text textMesh;

    void Start()
    {
        if (textMesh != null)
        {
            textMesh.text = "";
        }
    }

    void OnEnable()
    {
        Application.logMessageReceived += LogMessage;

    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogMessage;
    }

    public void LogMessage(string message, string stackTrace, LogType type)
    {
        if (textMesh == null)
            textMesh = gameObject.GetComponentInChildren<TMP_Text>();

        if (textMesh == null)
            return;

        message = System.DateTime.Now.ToString("G") + ": " + message;
        if (textMesh.text.Length > 1000)
        {
            textMesh.text = textMesh.text.Substring(textMesh.text.Length - 1000, 1000) + message + "\n";
        }
        else
        {
            textMesh.text += message + "\n";
        }
    }
}
