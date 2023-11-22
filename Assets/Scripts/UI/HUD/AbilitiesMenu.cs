using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitiesMenu : UI
{
    [SerializeField] private AbilitiesSprite[] abilitiesSprites;

    private void Start()
    {
        GameEventsManager.Instance.PlayerEvents.OnUpdateAbility += PlayerEventsOnOnUpdateAbility;
    }

    private void PlayerEventsOnOnUpdateAbility(int idx, AbilitySO newAbility)
    {
        abilitiesSprites[idx].UpdateAbilitySlot(newAbility);
    }
}