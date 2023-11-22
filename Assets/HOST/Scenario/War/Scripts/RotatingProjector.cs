using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingProjector : MonoBehaviour
{
    public bool IsRotating;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsRotating)
        {
            

            gameObject.transform.RotateAround(transform.position, transform.forward, 2000 * Time.deltaTime);
            
        }
    }
}
