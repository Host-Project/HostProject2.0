using HOST.Scenario;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Element), true)]
public class ElementEditor : HostNetworkRPCEditor
{
    override public void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        //SetRPCId();
        base.OnInspectorGUI();
        Element element = target as Element;
        if (GUILayout.Button("Complete"))
        {
            element.RequestComplete();
        }
    }
}
