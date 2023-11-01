using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Influencers.Interactors
{
    public class AudioInteractor : InfluenceInteractor
    {
        [SerializeField]
        private AudioSource audioSource;
       

        public void PlayAudio(AudioClip audioClip, float duration)
        {
            Interact(duration);
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        protected override void StopInteraction()
        {
            base.StopInteraction();

            audioSource.Stop();
        }
    }

}