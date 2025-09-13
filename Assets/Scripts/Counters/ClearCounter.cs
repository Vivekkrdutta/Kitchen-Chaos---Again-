using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    #region Interact Mechanism
    public override void Interact(Player player)
    {
        if(!HasKitchenObject()) // Does not have it
        {
            if (player.HasKitchenObject())
            {
                Debug.Log(player.GetKitchenObject().GetKitchenObjectSO());
                // Take from the player
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
        }
        else // It has the KitchenObject
        {
            if (!player.HasKitchenObject())
            {
                // Give it to the player
                GetKitchenObject().SetKitchenObjectParent(player);
                return;
            }

            else // Means player has something
            {
                // If the player is holding a plate
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate))
                {
                    if (plate.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                }
                else // Means player has some food item
                {
                    if(GetKitchenObject().TryGetPlate(out plate))
                    {
                        // The table has a plate
                        if (plate.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            // Successfully landed the food on the plate (over the counter, from player)
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            }
        }
    }

    #endregion
}