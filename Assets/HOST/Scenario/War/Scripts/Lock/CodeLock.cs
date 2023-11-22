using FMETP;
using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CodeLock : HostNetworkRPC
{
    [SerializeField]
    protected string expectedCode;

    [SerializeField]
    private bool useNetwork = true;

    public UnityEvent OnCodeValid;

    public UnityEvent OnCodeInvalid;



    public void RequestTryCode(string code)
    {
        if (useNetwork)
        {
            if (FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
            {
                HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                {
                    InstanceId = this.InstanceId,
                    MethodName = "TryCode",
                    Parameters = new object[] { code }
                });
                TryCode(code);
            }
            else
            {
                HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
                {
                    InstanceId = this.InstanceId,
                    MethodName = "RequestTryCode",
                    Parameters = new object[] { code }
                });
            }
        }
        else
        {
            TryCode(code);
        }
    }
    protected void TryCode(string code)
    {
        if (code == expectedCode)
        {
            OnCodeValid.Invoke();
        }
        else
        {
            OnCodeInvalid.Invoke();
        }
    }
}
