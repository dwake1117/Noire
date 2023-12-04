using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHeavyAttack", menuName = "Abilities/PlayerHeavyAttack")]
public class PlayerHeavyAttack : AbilitySO
{
    [SerializeField] private float attackDuration = 0.4f;
    [SerializeField] private float overChargeDuration = 3f;
    [SerializeField] private string releaseAnimationTrigger;
    private float initialTimer;
    private bool charging;
    private Coroutine overchargeCoroutine;
    
    protected override void Cast()
    {
        initialTimer = Time.time;
        
        if(overchargeCoroutine != null) 
            Player.Instance.StopCoroutine(overchargeCoroutine);
        
        Player.Instance.ResetAnimationTrigger(releaseAnimationTrigger);
        Player.Instance.SetAnimatorTrigger(abilityAnimationTrigger);
        Player.Instance.ChargeSwordAnimation();
        
        overchargeCoroutine = Player.Instance.StartCoroutine(OverCharge());
        
        charging = true;
    }

    private void Release()
    {
        Player.Instance.HandleAttackOnHitEffects(abilityDamage, attackDuration);
        Player.Instance.SetAnimatorTrigger(releaseAnimationTrigger);
        Player.Instance.MoveFor(20f, 0.2f, Player.Instance.transform.forward);
    }

    protected override void Interrupt()
    {
        Player.Instance.StopChargeSwordAnimation();
        float charge = Time.time - initialTimer;
        if (charge >= 1)
        {
            state = AbilityState.OnCooldown;
            Player.Instance.StartCoroutine(Player.Instance.WaitForAndReset(attackDuration));
            Release();
        }
        else
        {
            // cancel!
            state = AbilityState.Ready;
            cooldownCounter = cooldown;
            Player.Instance.ResetStateAfterAction();
        }
        charging = false;
    }

    private IEnumerator OverCharge()
    {
        yield return new WaitForSeconds(overChargeDuration);
        if (charging)
        {
            state = AbilityState.OnCooldown;
            Player.Instance.StopChargeSwordAnimation();
            Player.Instance.ResetStateAfterAction();
        }

        overchargeCoroutine = null;
    }
}
