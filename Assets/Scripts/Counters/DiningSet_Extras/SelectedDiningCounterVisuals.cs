using UnityEngine;

public class SelectedDiningCounterVisuals : SelectedCounterVisual
{
    [SerializeField] private Material selectedMaterial;
    private Material[] originalVisualsMaterials;

    protected override void Start()
    {
        base.Start();
        originalVisualsMaterials = new Material[visualGameObjects.Length];
        for (int i = 0; i < visualGameObjects.Length; i++)
        {
            MeshRenderer renderer = visualGameObjects[i].GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                originalVisualsMaterials[i] = renderer.material;
            }
        }
    }

    protected override void Show()
    {
        ChangeMaterialOfSelectedVisualsToSelctedMaterial();
    }

    private void ChangeMaterialOfSelectedVisualsToSelctedMaterial()
    {
        if (selectedMaterial != null)
        {

            foreach (Transform t in visualGameObjects)
            {
                //t.localScale *= 1.01f;
                MeshRenderer renderer = t.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = selectedMaterial;
                }
            }
        }
    }

    protected override void Hide()
    {
        ChangeMaterialOfSelectedVisualsToDefaultMaterial();
    }
    private void ChangeMaterialOfSelectedVisualsToDefaultMaterial()
    {

        for (int i = 0; i < visualGameObjects.Length; i++)
        {
           
            //visualGameObjects[i].localScale /= 1.01f;
            MeshRenderer renderer = visualGameObjects[i].GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = originalVisualsMaterials[i];
            }
        }
    }

}
