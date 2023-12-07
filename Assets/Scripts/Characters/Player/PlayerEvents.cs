using System;
using UnityEngine;

public class PlayerEvents
{
    /// Event for player taking any form of damage.
    public void TakeDamage(int value, Vector3 source) => OnTakeDamage?.Invoke(value, source);
    public event Action<int, Vector3> OnTakeDamage;
    
    /// Event for player healing
    public void RegenHealth(int value) => OnHealthRegen?.Invoke(value);
    public event Action<int> OnHealthRegen;
    
    /// Event for updating dream shards. Adds `value` to current shards.
    public void DreamShardsChange(float value) => OnDreamShardsChange?.Invoke(value);
    public event Action<float> OnDreamShardsChange;
    
    /// Event for updating dream threads. Adds `value` to current threads.
    public void DreamThreadsChange(float value) => OnDreamThreadsChange?.Invoke(value);
    public event Action<float> OnDreamThreadsChange;
    
    /// Called after shards has been actually updated in playerShardsSO
    public void DreamShardsChangeFinished() => OnDreamShardsChangeFinished?.Invoke();
    public event Action OnDreamShardsChangeFinished;
    
    /// Called after threads has been actually updated in playerShardsSO
    public void DreamThreadsChangeFinished() => OnDreamThreadsChangeFinished?.Invoke();
    public event Action OnDreamThreadsChangeFinished;

    public void UpdateHealthBar() => OnUpdateHealthBar?.Invoke();
    public event Action OnUpdateHealthBar;
    
    public void UpdateStaminaBar() => OnUpdateStaminaBar?.Invoke();
    public event Action OnUpdateStaminaBar;
    
    /// Event for updating ability slots for whatever reason (dream state changes, etc)
    /// Updates the `idx`th slot with `ability`
    public void UpdateAbility(int idx, AbilitySO ability) => OnUpdateAbility?.Invoke(idx, ability);
    public event Action<int, AbilitySO> OnUpdateAbility;
}
