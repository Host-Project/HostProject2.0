using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissingPiece : MonoBehaviour
{

    public GameObject piece;

    public UnityEvent OnPieceCollected;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        if(other.gameObject == piece && HostNetworkManager.instance.IsServer())
        {
            OnPieceCollected.Invoke();
        }
    }
}
