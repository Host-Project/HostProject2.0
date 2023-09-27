using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameObject : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private HostNetworkManager hostNetworkManager = null;

    [SerializeField]
    private bool moving = false;

    public int Id { get; set; }

    void Start()
    {
        InvokeRepeating("Sync", 0.01f, 0.01f);
    }


    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            this.transform.Translate(Vector3.forward * Time.deltaTime*10);
            this.transform.Rotate(new Vector3(0,1,0));
        }
    }

    public void Sync()
    {
        if (hostNetworkManager == null) { throw new System.Exception("[NetworkGameObject - Sync] HostNetworkManager not set."); }
        hostNetworkManager.SyncGameObject(this);
    }
}
