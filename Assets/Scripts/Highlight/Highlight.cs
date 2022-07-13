using UnityEngine;
using System.Linq;

/// <summary>
/// Highlights every mesh under the hierarchy of the attached transform if enabled.
/// </summary>
public class Highlight : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Color highlightColor;
    [SerializeField] [Range(0f, 1f)] private float highlightAlpha = 0.2f;
    private Renderer[] renderers;
    private Material instanceMaterial;
    private string highlightShaderColorPropertyName = "_Color";
    private string highlightShaderAlphaPropertyName = "_Alpha";
    private string highlightMaterialName => instanceMaterial.name + " (Instance)";

    #endregion

    #region Methods

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        CombineSubmeshes();
        instanceMaterial = Instantiate(highlightMaterial);
        instanceMaterial.SetColor(highlightShaderColorPropertyName, highlightColor);
        instanceMaterial.SetFloat(highlightShaderAlphaPropertyName, highlightAlpha);
        instanceMaterial.name = "HighlightMaterial";
    }

    private void OnEnable()
    {
        foreach (var renderer in renderers)
        {
            var materials = renderer.materials.ToList();
            materials.Add(instanceMaterial);
            renderer.materials = materials.ToArray();
        }
    }

    private void OnDisable()
    {
        foreach (var renderer in renderers)
        {
            var materials = renderer.materials.ToList();
            for (int i = 0; i< materials.Count; i++)
            {
                if(materials[i].name == highlightMaterialName)
                {
                    materials.RemoveAt(i);
                    i--;
                }
            }
            renderer.materials = materials.ToArray();
        }
    }

    /// <summary>
    /// Adds a new submesh to meshes with multiple submeshes to apply the highlight on the whole mesh.
    /// WARNING: This function writes to the shared mesh of the renderers.
    /// </summary>
    private void CombineSubmeshes()
    {
        foreach (var r in renderers)
        {
            Mesh mesh = null;
            if (r is SkinnedMeshRenderer)
                mesh = (r as SkinnedMeshRenderer).sharedMesh;
            else if (r is MeshRenderer)
                mesh = r.GetComponent<MeshFilter>().sharedMesh;
            if (mesh != null && mesh.subMeshCount > 1 && mesh.subMeshCount <= r.materials.Length)
            {
                mesh.subMeshCount++;
                mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
            }
        }
    }

    #endregion
}
