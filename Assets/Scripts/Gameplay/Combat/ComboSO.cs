using UnityEngine;

[CreateAssetMenu(fileName = "Combo", menuName = "Combo")]
public class ComboSO : ScriptableObject
{
    [SerializeField] public AbilitySO[] abilities;
    [SerializeField] public AnimatorOverrideController[] animations;
}