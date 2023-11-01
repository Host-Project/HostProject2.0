using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOST.Influencers.Interactors
{
    public class ObjectInteractor : InfluenceInteractor
    {
        private GameObject obj;
        public void Active(GameObject obj)
        {
            Interact();
            this.obj = obj;
            obj.SetActive(true);
        }

        override protected void StopInteraction()
        {
            base.StopInteraction();
            this.obj.SetActive(false);
            this.obj = null;
        }
    }
}