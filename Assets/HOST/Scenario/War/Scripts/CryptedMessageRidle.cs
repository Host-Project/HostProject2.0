using FMETP;
using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CryptedMessageRidle : HostNetworkRPC
{
    [SerializeField]
    private List<TextMeshProUGUI> keys;

    [SerializeField]
    private GameObject messageObject;
    private Quaternion rotation;
    [SerializeField]
    private TextMeshProUGUI message;

    [SerializeField]
    private Transform inPosition;
    [SerializeField]
    private Transform outPosition;

    [SerializeField]
    private AudioSource doorSound;

    [SerializeField]
    private Toggle VanocomycineCheck;

    [SerializeField]
    private Toggle CephalosporineCheck;

    [SerializeField]
    private Toggle AmoxicilineCheck;

    public UnityEvent OnDone;

    public UnityEvent OnFailed;



    private void Start()
    {
        base.Start();
        rotation = messageObject.transform.rotation;
    }
    private void Update()
    {
       
    }


    private (string, string) CryptMessage()
    {

        string uncrypted = "il est allergique a la cephalosporine";

        string remainingLetters = "abefghjklnopqrstwxyz";
        string lettersToCode = "lgquchons";

        Dictionary<char, char> pairs = new Dictionary<char, char>
        {
            { 'e', 'c' },
            { 'i', 'u' },
            { 't', 'd' },
            { 'r', 'i' },
            { 'a', 'm' },
            { 'p', 'v' }
        };

        string pairsString = string.Empty;

        foreach (char c in lettersToCode)
        {

            char l = remainingLetters[UnityEngine.Random.Range(0, remainingLetters.Length - 1)];
            pairs.Add(c, l);
            remainingLetters = remainingLetters.Replace(l.ToString(), string.Empty);

            pairsString += c + " = " + l + ";";

        }

        pairsString = pairsString.Substring(0, pairsString.Length - 1);

        string crypted = string.Empty;

        foreach (char c in uncrypted)
        {
            try
            {
                crypted += pairs[c];
            }
            catch (System.Exception e)
            {
                crypted += " ";
            }
        }


        return (crypted, pairsString);
    }


    public void GenerateMessage()
    {
        if (HostNetworkManager.instance.IsServer())
        {
            (string crypted, string pairs) = CryptMessage();
            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "SetCryptedMessage",
                Parameters = new object[] { crypted, pairs }
            });
            SetCryptedMessage(crypted, pairs);
        }
    }

    public void SetCryptedMessage(string message, string pairs)
    {

        this.message.GetComponentInChildren<TextMeshProUGUI>().text = message;

        string[] lst = pairs.Split(';');
        for (int i = 0; i < lst.Length; ++i)
        {
            keys[i].text = lst[i];
        }
    }

    public void GiveMessage()
    {
        if (HostNetworkManager.instance.IsServer())
        {

            HostNetworkManager.instance.SendRPC(new HostNetworkRPCMessage()
            {
                InstanceId = this.InstanceId,
                MethodName = "GiveMessage",
                Parameters = new object[] { }
            });
            
        }
        messageObject.transform.localPosition = inPosition.localPosition;
        messageObject.transform.rotation = rotation;
        messageObject.GetComponent<AudioSource>().Play();
        doorSound.Play();
    }



    public void CheckAnswer()
    {
        messageObject.transform.localPosition = outPosition.localPosition;

        if (HostNetworkManager.instance.IsServer())
        {
            if (!CephalosporineCheck.isOn || VanocomycineCheck.isOn || AmoxicilineCheck.isOn)
            {
                OnFailed.Invoke();
                Invoke("GiveMessage", 5);
            }
            else
            {
                OnDone.Invoke();
            }
        }
    }


}
