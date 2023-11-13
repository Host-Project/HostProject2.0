using Meta.WitAi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace HOST.Interactions.Common
{

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Movable : MonoBehaviour
    {

        private bool hasMoved = false;
        private Rigidbody rb;

        public UnityEvent<GameObject> OnMovementStart;
        public UnityEvent<GameObject> OnMovementUpdate;
        public UnityEvent<GameObject> OnMovementEnd;



        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }
        void Update()
        {

            if (hasMoved && rb.angularVelocity.magnitude == 0f && rb.velocity.magnitude == 0)
            {
                OnMovementEnd.Invoke(gameObject);
                hasMoved = false;
            }
            else if (rb.angularVelocity.magnitude != 0f || rb.velocity.magnitude != 0f)
            {
                if (!hasMoved)
                    OnMovementStart.Invoke(gameObject);
                else
                    OnMovementUpdate.Invoke(gameObject);
                hasMoved = true;
            }

        }
    }
}