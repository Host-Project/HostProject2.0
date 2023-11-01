using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissingBatteryCheck : MonoBehaviour
{
    public GameObject WantedObject;

    public UnityEvent ItemFound;

    public GameObject Model;

    public void Start()
    {
        Model.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {

        if(WantedObject.GetInstanceID() == other.gameObject.GetInstanceID())
        {
            ItemFound.Invoke();
        }
    }

    public void OnComplete()
    {
        WantedObject.SetActive(false);

        Model.SetActive(true);
    }
}
