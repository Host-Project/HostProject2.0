using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Influencers
{
    public class InfluencerManager : MonoBehaviour
    {
        public UnityEvent OnPlaySound;
        public UnityEvent OnStopSound;

        public UnityEvent OnPlayAnimation;
        public UnityEvent OnStopAnimation;

        public UnityEvent OnShowText;
        public UnityEvent OnHideText;

        public UnityEvent OnShowImage;
        public UnityEvent OnHideImage;

        public void PlaySound(AudioClip audio, float duration = 25)
        {
            OnPlaySound.Invoke();
            Debug.Log("Play Sound");
            CancelInvoke("StopSound");
            Invoke("StopSound", duration);
        }


        private void StopSound()
        {
            OnStopSound.Invoke();
            Debug.Log("Stop Sound");
        }

        public void PlayAnimation()
        {
            OnPlayAnimation.Invoke();
            CancelInvoke("StopAnimation");
            Invoke("StopAnimation", 25);
        }

        public void ShowText(string text)
        {
        }

        public void ShowImage(Sprite image, float duration = 25)
        {

        }

        private void HideImage()
        {

        }
    }
}