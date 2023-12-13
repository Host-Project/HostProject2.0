using HOST.Networking;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HostNetworkObject), true)]
public class HostNetworkObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SetRPCId();
    }

    private void SetRPCId()
    {
        HostNetworkObject instance = target as HostNetworkObject;

        var existing = FindObjectsByType<HostNetworkObject>(FindObjectsSortMode.None);

        existing = existing.Where(x => x.Id == instance.Id).ToArray();

        while (instance.Id == -1 || existing.Length > 1)
        {
            instance.Id++;
            existing = existing.Where(x => x.Id == instance.Id).ToArray();
        }
    }
}