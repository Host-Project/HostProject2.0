using FMETP;
using HOST.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostNetworkObject : MonoBehaviour
{
    public int Id { get; set; }

    public HostNetworkObjectTransform GetTransform()
    {
        return new HostNetworkObjectTransform()
        {
            Id = Id,
            Position = transform.position,
            Rotation = transform.rotation,
            Scale = transform.localScale
        };
    }

    public void SetTransform(HostNetworkObjectTransform transform)
    {
        if (this.transform.hasChanged && FMNetworkManager.instance.NetworkType == FMNetworkType.Client)
        {
            HostNetworkManager.instance.RequestObjectSync(this.GetTransform());
            this.transform.hasChanged = false;
            return;
        }
        this.transform.position = transform.Position;
        this.transform.rotation = transform.Rotation;
        this.transform.localScale = transform.Scale;
        this.transform.hasChanged = false;
    }
   
}
