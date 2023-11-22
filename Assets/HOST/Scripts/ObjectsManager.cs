using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsManager : MonoBehaviour
{

    [SerializeField]
    private List<GameObject> clientsObjects = new List<GameObject>();
    [SerializeField]
    private List<GameObject> serversObjects = new List<GameObject>();
    void Awake()
    {
        if (HostNetworkManager.instance.IsServer())
        {
            clientsObjects.ForEach(x => x.SetActive(false));
        }
        else
        {
            serversObjects.ForEach(x => x.SetActive(false));
        }
    }

    void Start()
    {
        Debug.Log("Start");
    }
}
