using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwitchBoxController : MonoBehaviour
{
    public List<SwitchController> Buttons;
    public List<bool> Password;

    public GameObject Door;

    public UnityEvent PasswordCorrect;

    public void OpenDoor()
    {
        Door.transform.DOLocalRotate(new Vector3(0f, 170f, 0f), 1.5f);
    }

    public void CloseDoor()
    {
        Door.transform.DOLocalRotate(new Vector3(0f, 0f, 0f), 1.5f);
    }

    public void CheckPassword()
    {
        if(Buttons.Count != Password.Count)
        {
            Debug.LogError("Buttons length is not equal to password length!");
        }

        for(int i = 0; i < Buttons.Count; i++)
        {
            if(Password[i] != Buttons[i].State)
            {
                return;
            }
        }

        // The password is correct, trigger event
        PasswordCorrect.Invoke();
    }
}
