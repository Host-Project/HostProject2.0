using MoenenGames.VoxelRobot;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Destroyable : MonoBehaviour
{

    public UnityEvent OnDestruction;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Bullet b))
        {
            OnDestruction.Invoke();
        }
    }
}
