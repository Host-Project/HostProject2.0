using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Influencers.Interactors
{
    public abstract class InfluenceInteractor : MonoBehaviour
    {
        public UnityEvent OnInteract;
        public UnityEvent OnStopInteraction;

        protected virtual void Interact(float duration=25)
        {
            OnInteract.Invoke();
            CancelInvoke("StopInteraction");
            Invoke("StopInteraction", duration);
        }

        protected virtual void StopInteraction()
        {
            OnStopInteraction.Invoke();
        }
    }
}