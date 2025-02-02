using System;
using UnityEngine;

public class AbilitiesMenu : UI
{
    [SerializeField] private AbilitiesSprite[] abilitiesSprites;

    private void Awake()
    {
        GameEventsManager.Instance.PlayerEvents.OnUpdateAbility += PlayerEventsOnOnUpdateAbility;
    }

    private void PlayerEventsOnOnUpdateAbility(int idx, AbilitySO newAbility)
    {
        abilitiesSprites[idx].UpdateAbilitySlot(newAbility);
    }
}