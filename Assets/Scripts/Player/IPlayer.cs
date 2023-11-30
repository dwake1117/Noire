using UnityEngine;

/// <summary>
/// Interface for player. Refer here for the available public functions for the Player Instance
/// </summary>

public interface IPlayer
{
    bool IsWalking();
    bool IsIdle();
    bool IsCasting();
    bool IsDead();
    bool IsFalling();
    bool IsRunning();
    float GetPlayerHitBoxHeight();
    Transform GetRangedTargeter();
    
    bool AddItem(CollectableItemSO item);
    bool RemoveItem(CollectableItemSO item);
    void SetMaxHP(int x);
    void SetMaxStamina(float x);
}