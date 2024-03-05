using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeTransform : MonoBehaviour
{
    [SerializeField] private bool freezePositionX;
    [SerializeField] private bool freezePositionY;
    [SerializeField] private bool freezePositionZ;

    private Vector3 initialPosition;
    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localPosition = transform.localPosition;
        if (freezePositionX)
        {
            localPosition.x = initialPosition.x;
        }
        if (freezePositionY)
        {
            localPosition.y = initialPosition.y;
        }
        if (freezePositionZ)
        {
            localPosition.z = initialPosition.z;
        }
        gameObject.transform.localPosition = localPosition;
    }
}
