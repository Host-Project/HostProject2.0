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


    enum Direction { In, Out, None }
    private Direction direction = Direction.None;



    private void Start()
    {
        base.Start();
        rotation = messageObject.transform.rotation;

    }
    private void Update()
    {
        if (!HostNetworkManager.instance.IsServer()) return;
        switch (direction)
        {
            case Direction.In:
                messageObject.transform.localPosition = Vector3.MoveTowards(messageObject.transform.localPosition, inPosition.localPosition, 0.1f);

                if (messageObject.transform.localPosition == inPosition.localPosition)
                {
                    direction = Direction.None;
                }
                break;
            case Direction.Out:
                messageObject.transform.localPosition = Vector3.MoveTowards(messageObject.transform.localPosition, outPosition.localPosition, 0.1f);

                if (messageObject.transform.localPosition == outPosition.localPosition)
                {
                    direction = Direction.None;

                }
                break;
        }
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
            messageObject.transform.localPosition = outPosition.localPosition;
            messageObject.transform.localRotation = rotation;
            direction = Direction.In;
        }
        doorSound.Play();
    }



    public void CheckAnswer()
    {
        if (direction != Direction.None || !HostNetworkManager.instance.IsServer()) return;

        messageObject.transform.localPosition = inPosition.localPosition;
        messageObject.transform.localRotation = rotation;
        direction = Direction.Out;
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
