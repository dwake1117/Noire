using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Player
{
    [Header("---------- Player Combat ---------- ")]
    [SerializeField] private Hitbox weaponHitbox;
    public Transform rangedTargeter;
    private readonly float playerHitBoxHeight = 1f;
    
    [Header("On Hit")]
    [SerializeField] private float invulnerableTimerMax = .5f;
    [SerializeField] private ParticleSystemBase onHitParticleEffects;
    [SerializeField] private float onHitParticleEffectsOffset;
    [SerializeField] private float knockedBackAnimTime = .4f;
    public float invulnerableTimer;
    private Coroutine onHitCoroutine;
    
    [Header("Abilities")]
    private AbilitySO[] playerAbilitiesList;  // up to three abilities is currently supported by input
    private Dictionary<int, AbilitySO> playerAbilities; // the available abilities we currently have given a dreamstate
    private AbilitySO currentAbility;
    
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
    
    // returns an AbilitySO if can cast `abilityId`, NULL otherwise
    public AbilitySO CanCastAbility(int abilityId)
    {
        if (!playerAbilities.TryGetValue(abilityId, out AbilitySO ability))
            return null;
        
        if ((ability.interruptable && currentAbility && abilityId == currentAbility.abilityID)
            || (playerStaminaSO.CurrentStamina >= ability.staminaCost && (IsIdle() || IsWalking() || IsRunning())))
        {
            return ability;
        }

        return null;
    }

    public void UseStamina(float val)
    {
        playerStaminaSO.UseStamina(val);
        GameEventsManager.Instance.PlayerEvents.UpdateStaminaBar();
    }
    
    private void OnAbilityCast(int abilityId, short status)
    {
        AbilitySO ability = CanCastAbility(abilityId);
        
        if (ability == null)
        {
            currentAbility = null;
            Debug.Log($"Ability cast {abilityId} failed: " +
                      "Already casting, not enough stamina, or ability not found");
        }
        else 
        {
            if(!ability.CanActivate(status))
                Debug.Log($"Ability {abilityId} not available, either on cooldown or locked");
            else
            {
                TryContinueOrStartCombo(abilityId);
                currentAbility = ability;
                
                // here we activate the ability at last!
                if (ability.Activate(status) && !ability.playerMovableDuringCast)
                {
                    state = PlayerState.Casting;
                }
            }
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
    
    /// called when player takes direct damage from a certain `source`
    private void OnHit(int dmg, Vector3 source)
    {
        if (invulnerableTimer > 0)
            return;

        // interrupts any interruptable abilities
        if (currentAbility && currentAbility.interruptable)
            currentAbility.Activate(-1);
        
        // take damage
        invulnerableTimer = invulnerableTimerMax;
        playerHealthSO.InflictDamage(dmg);
        GameEventsManager.Instance.PlayerEvents.UpdateHealthBar();
        
        // handle state changes
        HandleDreamState();
        if (playerHealthSO.IsDead())
        {
            // knock backs
            MoveFor(20, .1f, transform.position - source, false);
            transform.LookAt(source);
            Die();
            return;
        }

        // play on-hit effects (material change + animation + slow time + chromatic impulse)
        if (onHitCoroutine != null)
            StopCoroutine(onHitCoroutine);
        onHitCoroutine = StartCoroutine(PlayOnHitEffects(source));
    }

    private float[] OnHitAnimVars = new float[3];
    private void CacheOnHitAnimationVariables()
    {
        OnHitAnimVars[0] = invulnerableTimerMax * 0.75f;
        OnHitAnimVars[1] = invulnerableTimerMax * 0.05f;
        OnHitAnimVars[2] = invulnerableTimerMax * 0.1f;
    }
    
    /// plays any animations or vfx upon player taking direct damage
    private IEnumerator PlayOnHitEffects(Vector3 source)
    {
        TimeManager.Instance.DoSlowMotion(duration:.3f);
        PostProcessingManager.Instance.CAImpulse(.4f, 1f);
        
        // knock back, and resets it back to idle after certain period of time
        MoveFor(20, .02f, transform.position - source, false);
        transform.LookAt(source);
        state = PlayerState.KnockedBack;
        StartCoroutine(WaitForAndReset(knockedBackAnimTime, true));
        
        // particle effects
        onHitParticleEffects.transform.position = rangedTargeter.position;
        onHitParticleEffects.transform.LookAt(source);
        onHitParticleEffects.transform.position += onHitParticleEffects.transform.forward * -onHitParticleEffectsOffset;
        onHitParticleEffects.Restart();
        
        // camera shake on hit
        CameraManager.Instance.CameraShake(0.3f, 5f);
        
        // material change animation
        ChangeToOnHitMaterial();
        yield return new WaitForSeconds(OnHitAnimVars[0]);
        
        ChangeToOriginalMaterial();
        yield return new WaitForSeconds(OnHitAnimVars[1]);
        
        ChangeToOnHitMaterial();
        yield return new WaitForSeconds(OnHitAnimVars[1]);
        
        ChangeToOriginalMaterial();
        yield return new WaitForSeconds(OnHitAnimVars[1]);
        
        ChangeToOnHitMaterial();
        yield return new WaitForSeconds(OnHitAnimVars[2]);
        
        // reset back to original material
        ChangeToOriginalMaterial();
        
        onHitCoroutine = null;
    }
    
    // updates abilities according to dream states
    private void UpdateAbilities()
    {
        playerAbilities = new Dictionary<int, AbilitySO>();
        foreach (AbilitySO ability in playerAbilitiesList)
        {
            if (Array.Exists(ability.applicableDreamStates, elem => elem == dreamState))
            {
                playerAbilities.Add(ability.abilityID, ability);
                if(ability.equippable)
                    GameEventsManager.Instance.PlayerEvents.UpdateAbility(ability.abilityType, ability);
                ability.Ready();
            }
        }
    }

    /// Handles melee attack on hit effects given a specific `dmg` and `duration` of the ability
    public void HandleAttackOnHitEffects(int dmg, float duration)
    {
        StartCoroutine(AttackEffectCoroutine(dmg, duration));
    }

    private IEnumerator AttackEffectCoroutine(int dmg, float duration)
    {
        weaponHitbox.EnableHitbox();
        weaponHitbox.SetCurrentDamage(dmg);
        
        yield return new WaitForSecondsRealtime(duration);
        weaponHitbox.DisableHitbox();
    }
}