using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHeavyAttack", menuName = "Abilities/PlayerHeavyAttack")]
public class PlayerHeavyAttack : AbilitySO
{
    [SerializeField] private float attackDuration = 0.4f;
    [SerializeField] private string releaseAnimationTrigger;

    private float initialTimer;
    private bool isReleased;
    
    protected override void Cast()
    {
        isReleased = false;
        initialTimer = Time.time;
        
        Player.Instance.ResetAnimationTrigger(releaseAnimationTrigger);
        Player.Instance.SetAnimatorTrigger(abilityAnimationTrigger);
        Player.Instance.ChargeSwordAnimation();
    }

    private void Release()
    {
        Player.Instance.SetAnimatorTrigger(releaseAnimationTrigger);
        Player.Instance.MoveFor(20f, 0.2f, Player.Instance.transform.forward);
        Player.Instance.HandleAttackOnHitEffects();
    }
    
    protected override void Interrupt()
    {
        switch (abilityStatus)
        {
            case 1: // complete release
                if (!isReleased)
                {
                    Release();
                    Finish();
                    isReleased = true;
                }

                break;
            case -1: // early release
                float charge = Time.time - initialTimer;
                if (charge > 1)
                {
                    Release();
                }

                Finish();
                isReleased = true;
                
                break;
        }
    }
    
    protected override void Finish()
    {
        state = AbilityState.OnCooldown;
        Player.Instance.StopChargeSwordAnimation();
        Player.Instance.StartCoroutine(Player.Instance.WaitForAndReset(attackDuration));
    }
}
