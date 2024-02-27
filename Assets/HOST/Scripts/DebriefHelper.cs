using HOST.Debriefing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebriefHelper : MonoBehaviour
{
    [SerializeField]
    private GameObject debriefingManager;
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    private void OnSceneChange(Scene arg0, Scene arg1)
    {
        if(arg1.name == "03_Review")
        {
            foreach (Transform child in debriefingManager.transform)
            {
                if(child.gameObject != debriefingManager)
                    child.gameObject.SetActive(false);
            }
        }
    }

    public void OpenInExplorer()
    {

        Application.OpenURL(debriefingManager.GetComponent<DebriefingManager>()._directoryPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
