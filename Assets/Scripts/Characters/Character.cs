using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Baseclass for Enemies and Players
/// </summary>

public class Character : MonoBehaviour
{
    [Header("Mesh Renderers and OnHit Material")]
    [SerializeField] protected Renderer[] onHitRenderers;
    [SerializeField] protected Material onHitMaterial;
    private List<Material[]> originalMaterials;
    private List<Material[]> onHitMaterials;
    
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
}