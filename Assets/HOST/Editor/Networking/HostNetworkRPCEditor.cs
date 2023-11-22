using HOST.Monitoring.Settings;
using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(HostNetworkRPC), true)]
public class HostNetworkRPCEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SetRPCId();
    }

    private void SetRPCId()
    {
        HostNetworkRPC instance = target as HostNetworkRPC;

        var existing = FindObjectsByType<HostNetworkRPC>(FindObjectsSortMode.None);

        existing = existing.Where(x => x.InstanceId == instance.InstanceId).ToArray();

        while(instance.InstanceId == -1 || existing.Length > 1)
        {
            instance.InstanceId++;
            existing = existing.Where(x => x.InstanceId == instance.InstanceId).ToArray();
        }
    }
}
