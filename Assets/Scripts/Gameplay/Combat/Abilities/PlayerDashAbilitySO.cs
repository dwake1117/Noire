using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDashAbility", menuName = "Abilities/PlayerDash")]
public class PlayerDashAbilitySO : AbilitySO
{
    [Header("Ability-specific Fields")]
    [SerializeField] private float dashSpeed = 30;
    [SerializeField] private float dashTime = 1;

    protected override void Cast()
    {
        Player.Instance.SetAnimatorTrigger(abilityAnimationTrigger);
        Player.Instance.invulnerableTimer = 1f;
        Player.Instance.MoveFor(dashSpeed, dashTime);
        
        Player.Instance.StartCoroutine(Dash());
    }
    
    protected override void Finish()
    {
        state = AbilityState.OnCooldown;
        Player.Instance.ResetStateAfterAction();
    }

    private IEnumerator Dash()
    {
        yield return new WaitForSeconds(dashTime);
        Finish();
    }
}
