using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleVFXManager : MonoBehaviour
{
    public static ParticleVFXManager Instance { get; private set; }

    [SerializeField] private GameObject attractionParticlePrefab;

    private void Awake()
    {
        if (Instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// Instantiates a attraction particle system of a given `number` of particles
    /// which follows a transform `follow`. Instanced at `position`.
    public void InstantiateAttractionParticles(int num, Transform attractedTo, Vector3 position)
    {
        var ps = Instantiate(attractionParticlePrefab, position, Quaternion.identity).GetComponent<ParticlesAttract>();
        ps.Init(num, attractedTo);
    }
}