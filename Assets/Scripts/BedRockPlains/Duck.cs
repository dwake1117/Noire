using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class Duck : InteractableObject
{
    [SerializeField] private ParticleSystemBase firefliesBurst;
    public override void Interact()
    {
        interactionsOccured++;
        firefliesBurst.Restart();
        interactableIndicator.Stop();
    }
}