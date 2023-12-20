using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedContainerManipulator : MonoBehaviour
{
    public GameObject CabinetDoor;

    public GameObject InsideContent;

    public GameObject MainLock;

    public GameObject BiggerLock;


    public void Start()
    {
        InsideContent.SetActive(false);
    }

    public void OpenCabinet()
    {

        // The cabinet has been unlocked, the main lock will play an animation and silently close, whe should the follow by opening the cabinet
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(2f);
        
        sequence.Append(CabinetDoor.transform.DOLocalRotate(new Vector3(0f, 0f, -170f), 1.5f));

        MainLock.gameObject.SetActive(false);
        BiggerLock.gameObject.SetActive(false);

        // Show the content (was hiddent to prevent cheating by peaking inside)
        InsideContent.SetActive(true);
    }
}
