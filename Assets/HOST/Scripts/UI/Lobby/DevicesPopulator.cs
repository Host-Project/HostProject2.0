using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DevicesPopulator : MonoBehaviour
{
    public GameObject deviceUIPrefab;

    public Sprite NotReadyIcon;
    public Sprite ReadyIcon;

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

    public void SetReadyDevice(ClientDevice client)
    {
        foreach(Transform child in transform)
        {
            if(child.GetComponentInChildren<TMP_Text>().text == client.IP)
            {
                child.transform.GetChild(1).GetComponent<Image>().sprite = client.IsReady? ReadyIcon : NotReadyIcon;
                return;
            }
        }
    }
}
