using FMETP;
using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SwitchStateEvent : UnityEvent<int, bool>
{
}


public class SwitchController : HostNetworkRPC
{
    public GameObject OnButton;
    public GameObject OffButton;

    public int Index;

    public bool State = true;

    public SwitchStateEvent StateChanged;
    
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        OnButton.SetActive(State);
        OffButton.SetActive(!State);
    }

    public void SetState(bool state)
    {
        if (state == State)
            return;

        State = state;
        OnButton.SetActive(State);
        OffButton.SetActive(!State);
    }

    public void RequestToggle()
    {
        if(FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
        {
            ToggleState();
            return;
        }
        HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
        {
            InstanceId = this.InstanceId,
            MethodName = "ToggleState",
            Parameters = new object[] { }
        });

    }

    public void ToggleState()
    {
        if(FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "ToggleState",
                Parameters = new object[] { }
            });
        }

        State = !State;
        OnButton.SetActive(State);
        OffButton.SetActive(!State);

        StateChanged.Invoke(Index, State);
    }
}
