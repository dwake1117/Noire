using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    private Collider collider;
    private int currentDamage;
    private bool canTriggerTimeSlow;

    private void Awake()
    {
        collider = GetComponent<Collider>();
        DisableHitbox();
    }

    public void SetCurrentDamage(int dmg)
    {
        currentDamage = dmg;
    }
    
    public void EnableHitbox()
    {
        collider.enabled = true;
        canTriggerTimeSlow = true;
    }

    public void DisableHitbox()
    {
        collider.enabled = false;
        canTriggerTimeSlow = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            if(canTriggerTimeSlow)
            {
                canTriggerTimeSlow = false;
                TimeManager.Instance.DoSlowMotion(.2f, 0.03f);
            }
            other.GetComponent<Damagable>()?.TakeHit(currentDamage, Player.Instance.GetRangedTargeter().position);
        }
    }
}