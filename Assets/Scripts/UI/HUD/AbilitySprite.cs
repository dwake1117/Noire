using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitiesSprite : MonoBehaviour
{
    [SerializeField] private AbilitySO ability;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private void Update()
    {
        if (!ability)
            return;
        switch (ability.state)
        {
            case AbilitySO.AbilityState.Active:
                cooldownText.text = "Active";
                break;
            case AbilitySO.AbilityState.Ready:
                cooldownText.text = "Ready";
                break;
            case AbilitySO.AbilityState.OnCooldown:
                cooldownText.text = ability.cooldownCounter.ToString("n2");
                break;
        }
    }
    
    public void UpdateAbilitySlot(AbilitySO newAbility)
    {
        ability = newAbility;
        image.sprite = ability.sprite;
    }
}