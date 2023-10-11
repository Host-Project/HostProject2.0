using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOST.Scenario
{
    public class Element : MonoBehaviour
    {
        [SerializeField]
        private int weight = 1;

        private UnityEvent<Element> onComplete;

        public UnityEvent<Element> OnComplete { get => onComplete; set => onComplete = value; }

        private bool isCompleted = false;

        public int Weight { get => weight; set => weight = value; }


        public void Complete()
        {
            isCompleted = true;
            OnComplete.Invoke(this);
        }


        public bool IsCompleted()
        {
            return isCompleted;
        }


    }

}
