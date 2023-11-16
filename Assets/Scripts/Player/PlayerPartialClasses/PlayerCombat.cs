using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player
{
    [Header("Player Combat")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private float invulnerableTimerMax = .5f;
    [SerializeField] private ParticleSystemBase onHitParticleEffects;
    
    [Header("On Hit Materials")]
    [SerializeField] private Renderer cloakRenderer;
    [SerializeField] private Material cloakOnHitMaterial;
    [SerializeField] private Material cloakOriginalMaterial;
    
    [Header("Glowing Sword Effects")]
    [SerializeField] private Renderer weaponFabricRenderer;
    [SerializeField] private Material originalWeaponFabricMaterial;
    [SerializeField] private Material onAttackWeaponFabricMaterial;
    [SerializeField] private ParticleSystemBase glowSwordEndParticles;
    
    private AbilitySO[] playerAbilitiesList;  // up to three abilities is currently supported
    private Dictionary<int, AbilitySO> playerAbilities;
    private readonly float playerHitBoxHeight = 1f;
    public float invulnerableTimer;
    private Coroutine onHitCoroutine;
    private Coroutine glowswordCoroutine;
    
    private void OnAbilityCast(int abilityId)
    {
        var ability = CanCastAbility(abilityId);
        if (ability != null)
        {
            if (ability.Activate())
            {
                if(!ability.playerMovableDuringCast)
                    state = PlayerState.Casting;
                playerStaminaSO.UseStamina(ability.staminaCost);
                GameEventsManager.Instance.PlayerEvents.UpdateStaminaBar();
            }
            else
            {
                Debug.Log($"Ability {abilityId} not available, either on cooldown or locked");
            }
        }
        else
        {
            Debug.Log($"Ability cast {abilityId} failed, either already casting or not enough stamina");
        }
    }

    public void GlowSwordAnimation()
    {
        weaponFabricRenderer.material = onAttackWeaponFabricMaterial;
        if(glowswordCoroutine != null)
            StopCoroutine(glowswordCoroutine);
        glowswordCoroutine = StartCoroutine(OnAttackFinished());
    }

    IEnumerator OnAttackFinished()
    {
        yield return new WaitForSeconds(2.5f);
        glowSwordEndParticles.transform.position = weaponFabricRenderer.transform.position;
        glowSwordEndParticles.Restart();
        weaponFabricRenderer.material = originalWeaponFabricMaterial;
        
        glowswordCoroutine = null;
    }
    
    // called when taking any damage
    private void OnTakingDamage(int dmg, Vector3 source)
    {
        if (invulnerableTimer > 0)
        {
            Debug.Log("Invulnerable -- did not take hit: " + dmg);
            return;
        }

        // take damage
        invulnerableTimer = invulnerableTimerMax;
        playerHealthSO.InflictDamage(dmg);
        GameEventsManager.Instance.PlayerEvents.UpdateHealthBar();
        
        // play on-hit effects (material change + animation)
        if (onHitCoroutine != null)
            StopCoroutine(onHitCoroutine);
        onHitCoroutine = StartCoroutine(PlayOnHitEffects(source));
        
        // handle effects
        HandleDreamState();
        if (playerHealthSO.IsDead())
            HandleDeath();
    }

    private IEnumerator PlayOnHitEffects(Vector3 source)
    {
        // on hit particle effects
        if (!onHitParticleEffects)
        {
            Debug.LogError("Did not find onHitParticleEffects. This may be intentional");
            yield return null;
        }
        else
        {
            onHitParticleEffects.transform.LookAt(source);
            onHitParticleEffects.Restart();
            
            CameraManager.Instance.CameraShake(invulnerableTimerMax, 5f);
            
            // material change
            cloakRenderer.material = cloakOnHitMaterial;
            yield return new WaitForSeconds(invulnerableTimerMax + 0.3f);
            cloakRenderer.material = cloakOriginalMaterial;
        }
        
        onHitCoroutine = null;
    }
    
    private void UpdateAbilities()
    {
        playerAbilities = new Dictionary<int, AbilitySO>();
        foreach (AbilitySO ability in playerAbilitiesList)
        {
            if (Array.Exists(ability.applicableDreamStates, elem => elem == DreamState))
            {
                playerAbilities.Add(ability.abilityID, ability);
                ability.Ready();
            }
        }
    }
    
    // called after attacks
    public void HandleAttackOnHitEffects()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(weapon.GetAttackPoint().position, weapon.GetAttackRadius(), weapon.GetEnemyLayer());
        foreach (Collider enemy in hitEnemies)
        {
            enemy.GetComponent<BasicEnemy>()?.OnHit();
            enemy.GetComponent<Enemy>()?.OnHit();
        }
    }
}