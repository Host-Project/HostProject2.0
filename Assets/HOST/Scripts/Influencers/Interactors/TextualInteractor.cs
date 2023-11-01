using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace HOST.Influencers.Interactors
{
    public class TextualInteractor : InfluenceInteractor
    {
        [SerializeField]
        private TMP_Text textMesh;

        public void ShowText(string text)
        {
            Interact();
            textMesh.text = text;
        }
        protected override void StopInteraction()
        {
            base.StopInteraction();
            textMesh.text = string.Empty;
        }
    }
}