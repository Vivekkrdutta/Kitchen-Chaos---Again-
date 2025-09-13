using UnityEngine;

[CreateAssetMenu(fileName = "FryingRecipeSO", menuName = "Scriptable Objects/FryingRecipeSO")]
public class FryingRecipeSO : ScriptableObject
{
    [SerializeField] public KitchenObjectSO input;
    [SerializeField] public KitchenObjectSO output;
    [SerializeField] public float fryingTimeMax;
}
