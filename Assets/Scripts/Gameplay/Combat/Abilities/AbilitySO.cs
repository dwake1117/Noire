using UnityEngine;
using System.Collections;

public abstract class AbilitySO : ScriptableObject
{
    [SerializeField] public int abilityID;
    [SerializeField] protected string abilityAnimationTrigger;
    [SerializeField] protected float cooldown = 1;
    [SerializeField] public float staminaCost = 10f;
    [SerializeField] public DreamState[] applicableDreamStates;
    [SerializeField] public bool playerMovableDuringCast;
    
    // if an ability is interruptable, Activate() can be called multiple times (be careful with this!)
    [SerializeField] public bool interruptable;

    private float cooldownCounter;
    
    // IMPORTANT: -1 for canceled, 0 for started. Use other numbers accordingly!
    protected short abilityStatus; // the status of the ability, updated in Activate()
    
    protected enum AbilityState
    {
        Ready,
        Active,
        OnCooldown
    }

    protected AbilityState state;
    
    // this is called on game awake
    public virtual void Ready() => state = AbilityState.Ready;
    
    // Called on the FIRST time the ability is casted
    // should continue the ability by modifying the state of the object it is attached to
    // when finished, call finish()
    protected abstract void Cast();

    // when finished, should make state = AbilityState.OnCooldown
    // IMPORTANT: must call Player.Instance.ResetStateAfterAction() otherwise the ability is stuck forever
    protected abstract void Finish();

    // called when ability is activated and if `interruptable` flag is true
    protected virtual void Interrupt() { }

    // called whenever the ability is activated
    // returns true if the activation is NOT a interrupt, false if it is
    public bool Activate(short status)
    {
        if (state == AbilityState.Ready && status == 0)
        {
            abilityStatus = status;
            state = AbilityState.Active;
            cooldownCounter = cooldown;
            Cast();
        }
        else if (state == AbilityState.Active && interruptable)
        {
            abilityStatus = status;
            Interrupt();
            return false;
        }

        return true;
    }

    public bool CanActivate(short status)
    {
        return (state == AbilityState.Ready && status == 0) 
               || (state == AbilityState.Active && interruptable);
    }
    
    // called on each frame during which the ability is activated
    public void DecreaseCooldown()
    {
        if (state == AbilityState.OnCooldown)
        {
            cooldownCounter -= Time.deltaTime;
            if (cooldownCounter < 0)
            {
                state = AbilityState.Ready;
                cooldownCounter = cooldown;
            }
        }
    }
    
    // helpful coroutines
    protected IEnumerator WaitEndOfAction(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Finish();
    }
    
    protected IEnumerator WaitFor(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }
}
