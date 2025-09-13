using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer headMeshRenderer;
    [SerializeField] private MeshRenderer bodyMeshRenderer;

    Material material;
    private void Awake()
    {
        material = headMeshRenderer.material;
        headMeshRenderer.material = material;
        bodyMeshRenderer.material = material;
    }
    public void SetColor(Color color)
    {
        material.color = color;
    }
}
