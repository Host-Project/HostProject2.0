using FMETP;
using HOST.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostNetworkObject : MonoBehaviour
{
    public int Id { get; set; }

    private bool onStartGravity = false;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            onStartGravity = rb.useGravity;
        }
        if (!HostNetworkManager.instance.IsServer())
        {
            if (rb != null)
            {
                rb.useGravity = false;
            }

        }
    }


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
        if (this.transform.hasChanged && !HostNetworkManager.instance.IsServer())
        {
            HostNetworkManager.instance.RequestObjectSync(this.GetTransform());
            this.transform.hasChanged = false;
            return;
        }
        this.transform.position = transform.Position;
        this.transform.rotation = transform.Rotation;
        this.transform.localScale = transform.Scale;
        this.transform.hasChanged = false;
        if (onStartGravity)
        {
            CancelInvoke("UseGravity");
            Invoke("UseGravity", 0.2f);
        }
    }


    private void UseGravity()
    {
        if(rb != null)
        {
            rb.useGravity = onStartGravity;
        }
    }
}
