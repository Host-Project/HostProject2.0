using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SeatsAnimation : MonoBehaviour
{
    public GameObject Seats;

    public void HideSeats()
    {
        Seats.transform.DOLocalMoveY(-2f, 5f);
        GetComponent<AudioSource>().Play();
    }

    public void ShowSeats()
    {
        Seats.transform.DOLocalMoveY(0f, 5f);
        GetComponent<AudioSource>().Play();
    }
}
