using FMETP;
using HOST.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostNetworkObject : MonoBehaviour
{
    [SerializeField]
    private int id = 0;

    private bool onStartGravity = false;
    private Rigidbody rb;

    public int Id { get => id; set => id = value; }

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

    private void Update()
    {
        if (!HostNetworkManager.instance.IsServer() && this.transform.hasChanged)
        {
            HostNetworkManager.instance.RequestObjectSync(this.GetTransform());
        }
    }


    public HostNetworkObjectTransform GetTransform()
    {
        return new HostNetworkObjectTransform()
        {
            Id = Id,
            Position = transform.localPosition,
            Rotation = transform.localRotation,
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
        this.transform.localPosition = transform.Position;
        this.transform.localRotation = transform.Rotation;
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
        if (rb != null)
        {
            rb.useGravity = onStartGravity;
        }
    }
}
