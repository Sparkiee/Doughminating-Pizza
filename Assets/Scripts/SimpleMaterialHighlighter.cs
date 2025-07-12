using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMaterialHighlighter : MonoBehaviour
{
    [Tooltip("Material used when object is highlighted")]
    public Material highlightMaterial;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private bool isHighlighted = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            // Store a copy of the original materials
            originalMaterials[i] = renderers[i].materials;
        }
    }

    public void SetHighlight(bool state)
    {
        if (state == isHighlighted) return;
        isHighlighted = state;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (state)
            {
                // Append highlight material to existing materials
                Material[] combined = new Material[originalMaterials[i].Length + 1];
                originalMaterials[i].CopyTo(combined, 0);
                combined[combined.Length - 1] = highlightMaterial;
                renderers[i].materials = combined;
            }
            else
            {
                // Restore original materials
                renderers[i].materials = originalMaterials[i];
            }
        }
    }
}