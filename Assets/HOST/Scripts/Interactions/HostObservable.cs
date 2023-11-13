using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class HostObservable : MonoBehaviour
{

    [SerializeField]
    private float timeToStareAt = 1f;
    public UnityEvent<GameObject> OnStaredAt;
    public UnityEvent<GameObject> OnStaredAtComplete;

    private HostObservable lastObjectStaredAt = null;
    private float timeStaredAt = 0f;


    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        // if raycast hits, it checks if it hit an object with the tag Player
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            if (hit.collider.gameObject.TryGetComponent(out HostObservable observable) && observable == this)
            {
                if(lastObjectStaredAt == observable)
                {
                    timeStaredAt += Time.deltaTime;
                    if(timeStaredAt >= timeToStareAt)
                    {
                        OnStaredAtComplete.Invoke(gameObject);
                        timeStaredAt = 0f;
                    }
                }
                else
                {
                    lastObjectStaredAt = observable;
                    OnStaredAt.Invoke(gameObject);
                    timeStaredAt += 0f;
                }

            }
            else
            {
                lastObjectStaredAt = null;
                timeStaredAt = 0f;
            }
        }
    }
}
