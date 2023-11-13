using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Interactions.Meta
{
    public class ObjectGrabedEventSender : OneGrabFreeTransformer, ITransformer
    {
        public UnityEvent<GameObject> OnGrabStart;
        public UnityEvent<GameObject> OnGrabUpdate;
        public UnityEvent<GameObject> OnGrabEnd;

        public new void Initialize(IGrabbable grabbable)
        {
            base.Initialize(grabbable);
        }
        public new void BeginTransform()
        {
            base.BeginTransform();
            OnGrabStart.Invoke(this.gameObject);
        }

        public new void UpdateTransform()
        {
            base.UpdateTransform();
            OnGrabUpdate.Invoke(this.gameObject);
        }

        public new void EndTransform()
        {
            base.EndTransform();
            OnGrabEnd.Invoke(this.gameObject);
        }
    }
}