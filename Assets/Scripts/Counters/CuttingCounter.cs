using System;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter,IHasProgress
{
    public static event EventHandler OnAnyCut; // basically for the soundmanager
    public static new void ResetStaticData() // for clearing out all static vars when loading and re-loading.
    {
        OnAnyCut = null;
    }


    public event EventHandler OnCut;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOs;

    private int cuttingProgress = 0;
    public override void Interact(Player player)
    {
        if (!HasKitchenObject()) // Does not have it
        {
            if (player.HasKitchenObject())
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    // Inform the server that this player has put something on the counter
                    InteractLogicPlaceObjectOnCounterServerRpc();

                    // Take from the player
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                }
                else
                {
                    Debug.LogWarning("Cannot Place it");
                }
            }
        }
        else // It has the KitchenObject
        {
            // If player does not have anything
            if (!player.HasKitchenObject())
            {
                // Give it to the player
                GetKitchenObject().SetKitchenObjectParent(player);
            }
            else // Player has something
            {
                // Check if player has a plate
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate))
                {
                    // Check if it can be added to the plate
                    if (plate.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        // Destroy the kitchenObject
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc()
    {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }

    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc()
    {
        // Set the cutting counter to zero
        cuttingProgress = 0;

        // Trigger the ProgressChanged Event
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = 0f
        });
    }

    public override void InteractAlternate() // have to sync all clients across the network
    {
        if (HasKitchenObject())
        {
            // check if it can actually cut
            if (HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
            {
                CutServerRpc(); // Inform the server that the player is cutting
                TestCuttingProgressDoneServerRpc(); // Inform the server to check if the cutting progress has been done
            }
            else
            {
                Debug.LogWarning("Cannot cut it");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutServerRpc()
    {
        if (HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())){

            CutClientRpc(); // Inform all the clients that someone is cutting : PS> also check if there is something to cut as client cutting may have a poor connection and cut way too many times :>
        }
    }

    [ClientRpc]
    private void CutClientRpc()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOForInput(GetKitchenObject().GetKitchenObjectSO());

        // perform a cut operation
        OnAnyCut?.Invoke(this, EventArgs.Empty);

        KitchenObjectSO outputKOSO = cuttingRecipeSO.output;
        cuttingProgress++;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });

        OnCut?.Invoke(this, null);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc()
    {
        if (HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOForInput(GetKitchenObject().GetKitchenObjectSO());
            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
            {
                // First Destroy the KO
                KitchenObject.DestroyKitchenObject(GetKitchenObject());

                // Now spawn the gotten output;
                KitchenObject.SpawnKitchenObject(cuttingRecipeSO.output, this);
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        return GetCuttingRecipeSOForInput(inputKitchenObjectSO) != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOForInput(inputKitchenObjectSO);
        if(cuttingRecipeSO != null)
        {
            return cuttingRecipeSO.output;
        }
        return null;
    }
    private CuttingRecipeSO GetCuttingRecipeSOForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach(CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOs)
        {
            if(cuttingRecipeSO.input == inputKitchenObjectSO)
            {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}
