using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField]
    Transform target = null;

    bool isManipulating = false;
    Vector3 followOffset;

    void Awake()
    {
        followOffset = transform.position - target.transform.position;
    }

    void LateUpdate()
    {

        if (isManipulating && target != null)
        {
            var targetPosition = target.position + followOffset;
            transform.position += targetPosition - transform.position;
            transform.rotation = target.transform.rotation;
            transform.localScale = target.transform.localScale;
        }
    }

    public void StartManipulating()
    {
        Debug.Log("[Follow] - StartManipulation");
        isManipulating = true;
    }

    public void EndManipulating()
    {
        Debug.Log("[Follow] - EndManipulating");
        isManipulating = false;
    }
}
