using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Baseclass for any character including enemies, NPC, or Player
/// Includes a state machine and animator update functions
/// </summary>

public class Character : MonoBehaviour
{
    [Header("---------- Damagable ---------- ")]
    [SerializeField] protected Renderer[] onHitRenderers;
    [SerializeField] protected Material onHitMaterial;
    private List<Material[]> originalMaterials;
    private List<Material[]> onHitMaterials;
    public bool CanBeHit = false;
    
    private CharacterState state;
    protected Animator animator;
    
    [Header("Animator State IDs")]
    protected int WALK_ID;
    protected int IDLE_ID;
    protected int FALL_ID;
    protected int RUN_ID;
    protected int KNOCKBACK_ID;
    protected int DIE_ID;

    #region Public Inline Functions
    public bool IsWalking() => state == CharacterState.Walking;
    public bool IsIdle() => state == CharacterState.Idle;
    public bool IsCasting() => state == CharacterState.Casting;
    public bool IsDead() => state == CharacterState.Dead;
    public bool IsFalling() => state == CharacterState.Falling;
    public bool IsRunning() => state == CharacterState.Running;
    public bool IsKnockedBack() => state == CharacterState.KnockedBack;
    public void ToggleCanBeHit(bool val) => CanBeHit = val;

    #endregion

    protected void Awake()
    {
        animator = GetComponent<Animator>();
        state = CharacterState.Idle;
    }

    protected void InitializeOnHitRenderers()
    {
        originalMaterials = new();
        onHitMaterials = new();
        foreach (var meshRenderer in onHitRenderers)
        {
            originalMaterials.Add(meshRenderer.materials);
            onHitMaterials.Add(Enumerable.Repeat(onHitMaterial, meshRenderer.materials.Length).ToArray());
        }
    }
    
    protected void ChangeToOnHitMaterial()
    {
        for (int i = 0; i < onHitRenderers.Length; i++)
            onHitRenderers[i].materials = onHitMaterials[i];
    }

    protected void ChangeToOriginalMaterial()
    {
        for (int i = 0; i < onHitRenderers.Length; i++)
            onHitRenderers[i].materials = originalMaterials[i];
    }

    public void TakeHit(int dmg, Vector3 source)
    {
        if(CanBeHit)
            OnHit(dmg, source);
    }
    
    protected virtual void OnHit(int dmg, Vector3 source) {}

    protected void ChangeStateTo(CharacterState nextState)
    {
        state = nextState;
        AnimatorUpdate();
    }
    
    protected void AnimatorUpdate()
    {
        animator.SetBool(WALK_ID, IsWalking());
        animator.SetBool(IDLE_ID, IsIdle());
        animator.SetBool(FALL_ID, IsFalling());
        animator.SetBool(RUN_ID, IsRunning());
        animator.SetBool(KNOCKBACK_ID, IsKnockedBack());
        animator.SetBool(DIE_ID, IsDead());
    }
}