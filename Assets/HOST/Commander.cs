using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commander : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private GameObject myObject;

    public void UpdateTransform(string direction)
    {
        Debug.Log("UpdateTransform");
        Debug.Log("Received direction : " + direction);
        //myObject.TryGetComponent<Rigidbody>(out var rb);
        //m.velocity = JsonUtility.FromJson<Vector3>(direction) * 0.1f;
        myObject.transform.position = JsonUtility.FromJson<Vector3>(direction) ;
    }
}
