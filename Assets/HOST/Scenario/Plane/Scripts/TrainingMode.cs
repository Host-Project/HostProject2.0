using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingMode : MonoBehaviour
{
    public GameObject Training;
    public GameObject Session;
    public GameObject Sfx;

    public void SetupPlacementMode()
    {
        Training.SetActive(false);
        Session.SetActive(true);
        Sfx.SetActive(false);
    }

    public void SetupTrainingMode()
    {
        Training.SetActive(true);
        Session.SetActive(false);
        Sfx.SetActive(false);
    }

    public void TrainingDone()
    {
        Training.SetActive(false);
        Session.SetActive(true);
        Sfx.SetActive(true);
    }
}
