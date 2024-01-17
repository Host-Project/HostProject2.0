using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderDoor : MonoBehaviour
{
    [SerializeField]
    private GameObject waitingFor;

    [SerializeField]
    private CryptedMessageRidle ridle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter" + other.gameObject);
        if(other.gameObject == waitingFor && HostNetworkManager.instance.IsServer())
        {
            Debug.Log("CheckAnswer");
            ridle.CheckAnswer();
        }
    }
}
