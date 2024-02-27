using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCenter : MonoBehaviour
{
    private bool isPlaced = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SceneAnchor anchor = FindAnyObjectByType<SceneAnchor>();
        if (anchor != null && !isPlaced)
        {
            Debug.Log(anchor);
            isPlaced = true;
            transform.position = anchor.transform.position;
            transform.rotation = Quaternion.Euler(transform.rotation.x, anchor.transform.GetChild(0).rotation.y, transform.rotation.z);
            //new Quaternion.EulerAngles(anchor.transform.rotation.x, anchor.transform.rotation.y, anchor.transform.rotation.z);
            
        }
    }
}
