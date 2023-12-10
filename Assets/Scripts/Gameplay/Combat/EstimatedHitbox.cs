using System;
using UnityEngine;

public class EstimatedHitbox : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            other.GetComponent<Character>()?.ToggleCanBeHit(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            other.GetComponent<Character>()?.ToggleCanBeHit(false);
        }
    }
}