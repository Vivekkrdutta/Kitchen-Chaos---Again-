using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{
    [SerializeField] private PlatesCounter platesCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;

    private System.Collections.Generic.List<GameObject> platesVisualsList = new System.Collections.Generic.List<GameObject>();
    private void Start()
    {
        platesCounter.OnPlatesSpawned += PlatesCounter_OnPlatesSpawned;
        platesCounter.OnPlatesRemoved += PlatesCounter_OnPlatesRemoved;
    }

    private void PlatesCounter_OnPlatesRemoved(object sender, System.EventArgs e)
    {
        GameObject plateObject = platesVisualsList[platesVisualsList.Count - 1];
        platesVisualsList.Remove(plateObject);
        Destroy(plateObject);
    }

    private void PlatesCounter_OnPlatesSpawned(object sender, System.EventArgs e)
    {
        Transform platesTransform = Instantiate(plateVisualPrefab, counterTopPoint);

        float plateOffsetY = 0.1f;
        platesTransform.localPosition += plateOffsetY * platesVisualsList.Count * Vector3.up;
        platesVisualsList.Add(platesTransform.gameObject);
    }
}
