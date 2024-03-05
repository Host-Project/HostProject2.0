using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetworkAction : HostNetworkRPC
{
    public UnityEvent onActionTrigger;

    public void TriggerAction()
    {
        if (HostNetworkManager.instance.IsServer())
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "Action",
                Parameters = new object[] { }
            });
            Action();
        }
        else
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "TriggerAction",
                Parameters = new object[] { }
            });
        }

    }

    private void Action()
    {
        onActionTrigger.Invoke();
        Debug.Log("Action Triggered");
    }
}
