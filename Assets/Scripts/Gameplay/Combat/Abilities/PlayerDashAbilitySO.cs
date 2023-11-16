using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDashAbility", menuName = "Abilities/PlayerDash")]
public class PlayerDashAbilitySO : AbilitySO
{
    [Header("Ability-specific Fields")]
    [SerializeField] private float dashSpeed = 30;
    [SerializeField] private float dashTime = 1;

    protected override void Initialize()
    {
        Player.Instance.SetAnimatorTrigger(abilityAnimationTrigger);
    }

    protected override void Cast()
    {
        Player.Instance.StartCoroutine(Dash());
    }
    
    protected override void Finish()
    {
        state = AbilityState.OnCooldown;
        Player.Instance.ResetStateAfterAction();
    }

    private IEnumerator Dash()
    {
        Player.Instance.invulnerableTimer = 1f;
        Player.Instance.MoveFor(dashSpeed, dashTime);
        yield return new WaitForSeconds(dashTime);
        Finish();
    }
}
