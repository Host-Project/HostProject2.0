using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DevicesPopulator : MonoBehaviour
{
    public GameObject deviceUIPrefab;

    public void AddDevice(string ip)
    {
        GameObject device = Instantiate(deviceUIPrefab, transform);
        device.GetComponentInChildren<TMP_Text>().text = ip;
    }

    public void RemoveDevice(string ip)
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponentInChildren<TMP_Text>().text == ip)
            {
                Destroy(child.gameObject);
                return;
            }
        }
    }
}
