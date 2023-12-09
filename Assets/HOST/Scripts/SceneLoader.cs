using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : HostNetworkRPC
{

    public static SceneLoader instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        base.Start();
    }
    public void RequestLoadScene(string name)
    {
        if (HostNetworkManager.instance.IsServer())
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "LoadScene",
                Parameters = new object[] { name }
            });
            LoadScene(name);
        }
        else
        {
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "RequestLoadScene",
                Parameters = new object[] { name }
            });
        }
    }
    private void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }


}
