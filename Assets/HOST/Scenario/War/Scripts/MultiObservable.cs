using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Utilities;

public class MultiObservable : MonoBehaviour
{
    public UnityEvent OnAllObserved;

    public List<HostObservable> observables;

    private int count;

    private void Start()
    {
        foreach (var observable in observables)
        {
            observable.OnStaredAtComplete.AddListener(OnObserved);
        }
    }

    private void OnObserved(GameObject arg0)
    {
        if(++count == observables.Count)
        {
            OnAllObserved.Invoke();
        }
    }
}
