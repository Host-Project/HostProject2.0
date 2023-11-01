using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOST.Influencers.Interactors
{
    public class GraphicalInteractor : InfluenceInteractor
    {
        [SerializeField]
        private Image spriteRenderer;


        public void ShowImage(Sprite image)
        {
            Interact();
            spriteRenderer.sprite = image;
            spriteRenderer.color = Color.white;
        }

        protected override void StopInteraction()
        {
            base.StopInteraction();
            spriteRenderer.sprite = null;
            spriteRenderer.color = Color.clear;
        }
    }
}