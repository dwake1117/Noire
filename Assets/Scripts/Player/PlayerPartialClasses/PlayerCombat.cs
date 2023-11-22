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
    public float invulnerableTimer;
    private readonly float playerHitBoxHeight = 1f;
    
    // [Header("Abilities")]
    private AbilitySO[] playerAbilitiesList;  // up to three abilities is currently supported by input
    private Dictionary<int, AbilitySO> playerAbilities; // the available abilities we currently have given a dreamstate
    private AbilitySO currentAbility;
    
    [Header("On Hit Materials")]
    [SerializeField] private Renderer cloakRenderer;
    [SerializeField] private Material cloakOnHitMaterial;
    [SerializeField] private Material cloakOriginalMaterial;
    private Coroutine onHitCoroutine;
    
    [Header("Glowing Sword Effects")]
    [SerializeField] private Renderer weaponFabricRenderer;
    [SerializeField] private Material originalWeaponFabricMaterial;
    [SerializeField] private Material onAttackWeaponFabricMaterial;
    [SerializeField] private Material onChargeWeaponFabricMaterial;
    [SerializeField] private ParticleSystemBase glowSwordEndParticles;
    [SerializeField] private ParticleSystemBase chargingParticles;
    private Coroutine glowSwordCoroutine;
    private Coroutine chargeSwordCoroutine;
    
    // NOTE: the current combo system CANNOT overwrite the CDs of current abilities 
    [Header("Combo")]
    [SerializeField] private float comboTimerMax = 2.5f; // the time that you can continue the combo
    private float comboTimer;
    
    private ComboSO[] playerComboList; // list of combos
    private Dictionary<int, ComboSO> playerCombos; // the available combos for EACH abilityID
    private ComboSO currentCombo; // the current comboSO
    private int comboCounter; // which index are we at for the combo? [-1 if we are not in combo]

    private void InitializeAbilitiesAndCombos()
    {
        playerAbilitiesList = Resources.LoadAll<AbilitySO>("Player/Abilities");
        playerComboList = Resources.LoadAll<ComboSO>("Player/Combos");

        playerCombos = new();
        foreach (var combo in playerComboList)
        {
            playerCombos.TryAdd(combo.abilities[0].abilityID, combo);
        }
    }
    
    // called in Update() to decrease the comboTimer. Terminates the combo if comboTimer < 0
    private void HandleComboCooldowns()
    {
        if (currentCombo)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer < 0)
            {
                currentCombo = null;
                comboCounter = -1;
            }
        }
    }
    
    // called every frame to decrease cooldown and handle ability states
    private void HandleAbilityCooldowns()
    {
        foreach (AbilitySO ability in playerAbilities.Values)
            ability.DecreaseCooldown();
    }
    
    private void TryContinueOrStartCombo(int abilityId)
    {
        // if we are currently in a combo, try to continue the combo, or terminate if the combo fails
        if (currentCombo)
        {
            if (currentCombo.abilities[comboCounter + 1].abilityID == abilityId)
            {
                comboCounter++;
                animator.runtimeAnimatorController = currentCombo.animations[comboCounter];
                
                comboTimer = comboTimerMax;
            }
        }
        // if we are not in a combo, start a new combo
        else
        {
            playerCombos.TryGetValue(abilityId, out currentCombo);
            if (currentCombo)
            {
                comboCounter = 0;
                animator.runtimeAnimatorController = currentCombo.animations[comboCounter];
                comboTimer = comboTimerMax;
            }
        }

        // if we have reached the end of the combo, we terminate it
        if (currentCombo && comboCounter == currentCombo.animations.Length - 1)
        {
            currentCombo = null;
            comboCounter = -1;
        }
    }
    
    private void OnAbilityCast(int abilityId, short status)
    {
        Debug.Log($"{abilityId}, {status}");
        var ability = CanCastAbility(abilityId);
        if (ability != null)
        {
            if(!ability.CanActivate(status))
                Debug.Log($"Ability {abilityId} not available, either on cooldown or locked");
            else
            {
                TryContinueOrStartCombo(abilityId);
                currentAbility = ability;
                if (ability.Activate(status))
                {
                    if(!ability.playerMovableDuringCast)
                        state = PlayerState.Casting;
                    
                    playerStaminaSO.UseStamina(ability.staminaCost);
                    GameEventsManager.Instance.PlayerEvents.UpdateStaminaBar();
                }
            }
        }
        else
        {
            Debug.Log($"Ability cast {abilityId} failed: Already casting, not enough stamina, or ability not found");
        }
    }

    public void GlowSwordAnimation(short glowingMaterial)
    {
        if(glowingMaterial == 0)
            weaponFabricRenderer.material = onAttackWeaponFabricMaterial;
        if(glowingMaterial == 1)
            weaponFabricRenderer.material = onChargeWeaponFabricMaterial;

        if (glowSwordCoroutine != null)
            StopCoroutine(glowSwordCoroutine);
        glowSwordCoroutine = StartCoroutine(OnAttackFinished());
    }

    IEnumerator OnAttackFinished()
    {
        yield return new WaitForSeconds(comboTimerMax);
        glowSwordEndParticles.transform.position = weaponFabricRenderer.transform.position;
        glowSwordEndParticles.Restart();
        weaponFabricRenderer.material = originalWeaponFabricMaterial;
        
        glowSwordCoroutine = null;
    }
    
    public void ChargeSwordAnimation()
    {
        chargingParticles.Restart();
        
        if(chargeSwordCoroutine != null)
            StopCoroutine(chargeSwordCoroutine);
        chargeSwordCoroutine = StartCoroutine(ChargeOneStackAnimationCoroutine());
    }

    public void StopChargeSwordAnimation()
    {
        chargingParticles.Stop();
        if(chargeSwordCoroutine != null)
            StopCoroutine(chargeSwordCoroutine);
    }

    private IEnumerator ChargeOneStackAnimationCoroutine()
    {
        yield return new WaitForSeconds(1); // as specified in release threshold in HeavyAttackSO
        if (IsCasting() && currentAbility.abilityID == 5) // we are in the charged attack state
        {
            PostProcessingManager.Instance.CAImpulse();
            GlowSwordAnimation(1);
        }

        chargeSwordCoroutine = null;
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

    private float[] OnHitAnimVars = new float[3];
    private void CacheOnHitAnimationVariables()
    {
        OnHitAnimVars[0] = invulnerableTimerMax * 0.75f;
        OnHitAnimVars[1] = invulnerableTimerMax * 0.05f;
        OnHitAnimVars[2] = invulnerableTimerMax * 0.1f;
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
        }
        
        // camera shake on hit
        CameraManager.Instance.CameraShake(0.3f, 5f);
        
        // material change animation
        cloakRenderer.material = cloakOnHitMaterial;
        yield return new WaitForSeconds(OnHitAnimVars[0]);
        
        cloakRenderer.material = cloakOriginalMaterial;
        yield return new WaitForSeconds(OnHitAnimVars[1]);
        
        cloakRenderer.material = cloakOnHitMaterial;
        yield return new WaitForSeconds(OnHitAnimVars[1]);
        
        cloakRenderer.material = cloakOriginalMaterial;
        yield return new WaitForSeconds(OnHitAnimVars[1]);
        
        cloakRenderer.material = cloakOnHitMaterial;
        yield return new WaitForSeconds(OnHitAnimVars[2]);
        
        // reset back to original material
        cloakRenderer.material = cloakOriginalMaterial;
        
        onHitCoroutine = null;
    }
    
    // updates abilities according to dream states
    private void UpdateAbilities()
    {
        playerAbilities = new Dictionary<int, AbilitySO>();
        foreach (AbilitySO ability in playerAbilitiesList)
        {
            if (Array.Exists(ability.applicableDreamStates, elem => elem == DreamState))
            {
                playerAbilities.Add(ability.abilityID, ability);
                if(ability.equippable)
                    GameEventsManager.Instance.PlayerEvents.UpdateAbility(ability.abilityType, ability);
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