using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GarrotRidle : MonoBehaviour
{
    public UnityEvent OnDone;

    [SerializeField]
    private GameObject bed;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(bed != null && other.gameObject == bed)
        {
            OnDone.Invoke();
        }
    }
   
}
