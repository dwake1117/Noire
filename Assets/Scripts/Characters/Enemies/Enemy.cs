using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class Enemy : Damagable
{   
    [Header("Enemy Properties")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int shardsOnDeath;
    [SerializeField] private int threadsOnDeath;
    
    [Tooltip("The default enemy damage")]
    [SerializeField] protected int damage = 1;
    
    [Header("Enemy Effects")]
    [Tooltip("The particle effects that play when the enemy is hit")]
    [SerializeField] private ParticleSystemBase onHitParticleEffects;
    [SerializeField] private ParticleSystemBase onHitCutEffects;
    
    [Tooltip("Mesh Renderers to apply the on hit material to")]
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
        InitializeOnHitRenderers();
        
        if (onHitParticleEffects != null)
            onHitParticleEffects.Stop();
    }

    public virtual void Update() { }
    
    public override void OnHit(int dmg, Vector3 source)
    {
        
        PlayOnHitEffects(source);
        PlayOnHitSound();
        RecieveDamage(dmg);
    }
    protected virtual void PlayOnHitSound(){
        // keep empty. This solely exists to be extended
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
    
    private void PlayOnHitEffects(Vector3 source)  
    {
        if (onHit != null)
            StopCoroutine(onHit);
        onHit = StartCoroutine(OnHitEffectsCoroutine(source));
    }
    
    private IEnumerator OnHitEffectsCoroutine(Vector3 source)
    {
        // handles particle effects
        if (onHitParticleEffects != null)
        {
            onHitParticleEffects.transform.LookAt(source);
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
        GameEventsManager.Instance.PlayerEvents.DreamShardsChange(shardsOnDeath);
        GameEventsManager.Instance.PlayerEvents.DreamThreadsChange(threadsOnDeath);
        ParticleVFXManager.Instance.InstantiateAttractionParticles(
            shardsOnDeath / 10 + 2, 
            Player.Instance.GetRangedTargeter(), 
            transform.position);
        
        HandleDeath();
        gameObject.SetActive(false);
    }
}