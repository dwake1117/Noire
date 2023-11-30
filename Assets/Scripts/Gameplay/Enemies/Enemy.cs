using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{   
    [Header("Enemy Properties")]
    [SerializeField] private int maxHealth = 5;
    
    [Tooltip("The default enemy damage")]
    [SerializeField] protected int damage = 1;
    
    [Header("Enemy Effects")]
    [Tooltip("The particle effects that play when the enemy is hit")]
    [SerializeField] private ParticleSystemBase onHitParticleEffects;
    [SerializeField] private ParticleSystemBase onHitCutEffects;
    
    [Tooltip("The material that is applied to the enemy on hit")]
    [SerializeField] private Material onHitMaterial;
    
    [Tooltip("Mesh Renderers to apply the on hit material to")]
    [SerializeField] private MeshRenderer[] onHitRenderers;

    [SerializeField] private float onHitAnimationTime = .5f;
    
    // private unserialized fields
    private float health;
    private List<Material[]> originalMaterials;
    private List<Material[]> onHitMaterials;
    private Coroutine onHit;
    
    private void Awake()
    {
        health = maxHealth;
        CacheOnHitAnimationVariables();
    }

    public virtual void Start()
    {
        originalMaterials = new();
        onHitMaterials = new();
        foreach (var meshRenderer in onHitRenderers)
        {
            originalMaterials.Add(meshRenderer.materials);
            onHitMaterials.Add(Enumerable.Repeat(onHitMaterial, meshRenderer.materials.Length).ToArray());
        }
        
        if (onHitParticleEffects != null)
            onHitParticleEffects.Stop();
    }

    public virtual void Update() { }
    
    public void OnHit(int dmg)
    {
        PlayOnHitEffects();
        RecieveDamage(dmg);
    }

    private void RecieveDamage(int dmg)
    {
        // Debug.Log(health);
        health -= dmg;
        if(health <= 0)
            Die();
    }
    
    private float[] OnHitAnimVars = new float[3];
    private void CacheOnHitAnimationVariables()
    {
        OnHitAnimVars[0] = onHitAnimationTime * 0.75f;
        OnHitAnimVars[1] = onHitAnimationTime * 0.05f;
        OnHitAnimVars[2] = onHitAnimationTime * 0.1f;
    }
    
    private void PlayOnHitEffects()  
    {
        if (onHit != null)
            StopCoroutine(onHit);
        onHit = StartCoroutine(PlayOnHitEffectsWithDelay());
    }

    private void ChangeToOnHitMaterial()
    {
        for (int i = 0; i < onHitRenderers.Length; i++)
            onHitRenderers[i].materials = onHitMaterials[i];
    }

    private void ChangeToOriginalMaterial()
    {
        for (int i = 0; i < onHitRenderers.Length; i++)
            onHitRenderers[i].materials = originalMaterials[i];
    }
    
    private IEnumerator PlayOnHitEffectsWithDelay()
    {
        yield return new WaitForSeconds(.2f);
        
        // handles particle effects
        if (onHitParticleEffects != null)
        {
            onHitParticleEffects.transform.LookAt(Player.Instance.GetRangedTargeter());
            onHitCutEffects.Restart();
            onHitParticleEffects.Restart();
        }
        
        // play onHitAnimations
        CameraManager.Instance.CameraShake(0.1f, 3f);
        ChangeToOnHitMaterial();
        yield return new WaitForSeconds(OnHitAnimVars[0]);
        
        ChangeToOriginalMaterial();
        yield return new WaitForSeconds(OnHitAnimVars[1]);
        
        ChangeToOnHitMaterial();
        yield return new WaitForSeconds(OnHitAnimVars[1]);
        
        ChangeToOriginalMaterial();
        yield return new WaitForSeconds(OnHitAnimVars[2]);
        
        ChangeToOnHitMaterial();
        yield return new WaitForSeconds(OnHitAnimVars[2]);
        
        ChangeToOriginalMaterial();
        // set current coroutine to null
        onHit = null;
    }

    protected virtual void HandleDeath() { }
    
    private void Die()
    {
        //PlayDyingSound
        HandleDeath();
        gameObject.SetActive(false);
    }
}