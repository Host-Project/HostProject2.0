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
        AutoCenter anchor = FindAnyObjectByType<AutoCenter>();
        if (anchor != null && !isPlaced)
        {
            Debug.Log("Placing scene");
            Debug.Log(gameObject.name);

            transform.position = anchor.transform.position;
            transform.rotation = anchor.transform.rotation;
            isPlaced = true;

            if (hideSceneAnchor)
            {
                foreach(Transform child in anchor.transform)
                    child.gameObject.SetActive(false);
            }
        }
    }
}
