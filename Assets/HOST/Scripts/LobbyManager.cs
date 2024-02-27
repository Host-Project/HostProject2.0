using HOST.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : HostNetworkRPC
{

    [SerializeField]
    private FloatingHeadFollowingObject floatingUI;
    [SerializeField]
    private TMP_Text text;
    [SerializeField]
    private LookAtPlayer lookAtPlayer;

    public void ToggleLockRotation()
    {
        if(floatingUI.isFollowing)
        {
            floatingUI.Stay();
            lookAtPlayer.StopRotate();
            // Set floating UI to be in the center of the scene, but at the same height
            floatingUI.gameObject.transform.localPosition = new Vector3(0, floatingUI.gameObject.transform.localPosition.y, 0);
            floatingUI.gameObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
            text.text = "Anchor is set! Click again to unlock rotation.";

        }
        else
        {
            lookAtPlayer.Rotate();
            floatingUI.Follow();
            text.text = "Anchor is not set! Click to lock the rotation.";
        }

        
    }
}
