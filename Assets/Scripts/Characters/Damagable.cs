using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Baseclass for anything you can damage
/// </summary>

public class Damagable : MonoBehaviour
{
    [Header("---------- Damagable ---------- ")]
    [SerializeField] protected Renderer[] onHitRenderers;
    [SerializeField] protected Material onHitMaterial;
    private List<Material[]> originalMaterials;
    private List<Material[]> onHitMaterials;
    public bool CanBeHit = false;
    
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

    public void ToggleCanBeHit(bool val) => CanBeHit = val;

    public void TakeHit(int dmg, Vector3 source)
    {
        if(CanBeHit)
            OnHit(dmg, source);
    }
    
    public virtual void OnHit(int dmg, Vector3 source) {}
}