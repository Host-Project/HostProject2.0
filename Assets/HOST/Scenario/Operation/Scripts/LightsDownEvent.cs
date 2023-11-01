using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsDownEvent : MonoBehaviour
{
    [SerializeField]
    private List<Light> lights;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerEvent()
    {
        foreach(Light l in lights)
        {
            l.enabled = false;
        }
        Invoke("StopEvent", 20);
    }

    private void StopEvent()
    {
        foreach (Light l in lights)
        {
            l.enabled = true;
        }
    }
}
