using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace HOST.Influencers.Interactors
{
    public class VideoInteractor : InfluenceInteractor
    {
        [SerializeField]
        private VideoPlayer videoPlayer;
        public void ShowVideo(VideoClip videoClip)
        {
            Interact();
            videoPlayer.clip = videoClip;
            videoPlayer.Play();
        }

        override protected void StopInteraction()
        {
            base.StopInteraction();
            videoPlayer.Stop();
        }
    }
}