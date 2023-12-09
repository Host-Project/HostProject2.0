using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FfmpegUnity.Sample
{
    public class CheckFinished : MonoBehaviour
    {
        public Text TextUI;
        public FfmpegCommand TargetCommand;

        IEnumerator Start()
        {
            TextUI.text = "Status: Is Starting...";

            while (!TargetCommand.IsRunning)
            {
                yield return null;
            }

            TextUI.text = "Status: Is Running...";

            while (TargetCommand.IsRunning)
            {
                yield return null;
            }

            TextUI.text = "Status: Is Finished";
        }
    }
}
