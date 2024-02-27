using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenInExplorer : MonoBehaviour
{
    public void Open()
    {
        FindAnyObjectByType<DebriefHelper>().OpenInExplorer();
    }
}
