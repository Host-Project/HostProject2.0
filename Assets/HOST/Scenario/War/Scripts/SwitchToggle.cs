using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SwitchToggle : HostNetworkRPC
{
    public Toggle toggle;

    public void RequestSwitch()
    {
        if (HostNetworkManager.instance.IsServer())
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "Switch",
                Parameters = new object[] { }
            });
            Switch();
        }
        else
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage() { InstanceId = this.InstanceId, MethodName = "RequestSwitch", Parameters = new object[] { } });
        }
    }
    private void Switch()
    {
        toggle.isOn = !toggle.isOn;
    }
}
