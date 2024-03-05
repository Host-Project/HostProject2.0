using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp.Server;

public class FloatingHeadFollowingObject : MonoBehaviour
{

    public bool isFollowing = true;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
        if (isFollowing)
        {
            gameObject.transform.rotation = Camera.main.transform.rotation;
            gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        }
    }

    public void Follow()
    {
        isFollowing = true;
        Debug.Log(isFollowing);
    }

    public void Stay()
    {
        isFollowing = false;
        Debug.Log(isFollowing);
    }


}
