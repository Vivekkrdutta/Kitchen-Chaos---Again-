using UnityEngine;

[CreateAssetMenu(fileName = "CuttingRecipeSO", menuName = "Scriptable Objects/CuttingRecipeSO")]
public class CuttingRecipeSO : ScriptableObject
{
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public int cuttingProgressMax;
}
