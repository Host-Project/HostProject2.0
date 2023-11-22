using FMETP;
using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpinnableDigitLock : CodeLock
{
    

    public List<DigitSpinner> digitSpinners = new List<DigitSpinner>();


    protected override void Start()
    {
        base.Start();

        if(digitSpinners.Count != expectedCode.Length) throw new System.ArgumentOutOfRangeException("digitSpinners", digitSpinners.Count, "digitSpinners must be the same length as expectedCode");

        foreach(DigitSpinner spinner in digitSpinners)
        {
            spinner.OnDigitChanged.AddListener(CurrentCodeChanged);
        }
    }


    public void CurrentCodeChanged()
    {
        string currentCode = "";

        foreach(DigitSpinner spinner in digitSpinners)
        {
            currentCode += spinner.GetCurrentNumber().ToString();
        }
            RequestTryCode(currentCode);
    }
}
