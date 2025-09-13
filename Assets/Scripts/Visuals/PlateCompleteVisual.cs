using UnityEngine;
using System.Collections.Generic;

public class PlateCompleteVisual : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject plate;
    [SerializeField] private List<KitchenObjectSO_GameObject> kitchenObjectSO_GameObjectList;

    [System.Serializable]
    public struct KitchenObjectSO_GameObject
    {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject gameObject;
    }
    
    private void Start()
    {
        plate.OnIngredientAdded += Plate_OnIngredientAdded;
        foreach (var kitchenObjectSO_GameObject in kitchenObjectSO_GameObjectList)
        {
            kitchenObjectSO_GameObject.gameObject.SetActive(false);
        }
    }

    private void Plate_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach(var kitchenObjectSO_GameObject in kitchenObjectSO_GameObjectList)
        {
            if(kitchenObjectSO_GameObject.kitchenObjectSO == e.kitchenObjectSO)
            {
                kitchenObjectSO_GameObject.gameObject.SetActive(true);
            }
        }
    }
}
