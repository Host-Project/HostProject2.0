using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SceneAnchor : MonoBehaviour
{
    // Start is called before the first frame update


    void Start()
    {
        transform.GetChild(0).localScale = new Vector3(1, 1, 1);
        //Invoke("SetCenter", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    
}
