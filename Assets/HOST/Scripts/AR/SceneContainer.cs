using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneContainer : MonoBehaviour
{
    private bool isPlaced = false;
    [SerializeField]
    private bool hideSceneAnchor = false;
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
            Debug.Log("Placing scene");
            Debug.Log(gameObject.name);
            transform.position = anchor.transform.position;
            transform.rotation = Quaternion.Euler(transform.rotation.x, anchor.transform.GetChild(0).rotation.y, transform.rotation.z);
            //new Quaternion.EulerAngles(anchor.transform.rotation.x, anchor.transform.rotation.y, anchor.transform.rotation.z);
            isPlaced = true;

            if (hideSceneAnchor)
            {
                anchor.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
