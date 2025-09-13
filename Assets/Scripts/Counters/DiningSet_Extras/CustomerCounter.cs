using NUnit.Framework;
using System;
using Unity.Netcode;
using UnityEngine;

public class CustomerCounter : BaseCounter
{
    public new static void ResetStaticData()
    {
        OnAnyKicthenObjectRequested = null;
        OnAnyRecipeFailure = null;
        OnAnyRecipeSuccess = null;
    }
    public static CustomerCounter Instance { get; private set; }
    [SerializeField] private RecipeListSO recipeListSO;
    [SerializeField] private float waitTimerMax = 10f;
    private RecipeSO requestedRecipeSO;

    // Individual events

    public event EventHandler<OnPlayerInteractionEventArgs> OnPlayerInteraction;
    public class OnPlayerInteractionEventArgs : EventArgs
    {
        public InteractionResult interactionResult;
    }
    public enum InteractionResult
    {
        success,
        failure,
        empty,
    }
    public event EventHandler<int> OnRecipeRequested;
    // Static events
    public static event EventHandler<OnKitchenObjectRequestedEventArgs> OnAnyKicthenObjectRequested;
    public class OnKitchenObjectRequestedEventArgs : EventArgs
    {
        public RecipeSO recipeSO;
    }
    public static event EventHandler<OnRecipeDeliveryEventArgs> OnAnyRecipeSuccess;
    public static event EventHandler OnAnyRecipeFailure;
    public class OnRecipeDeliveryEventArgs : EventArgs
    {
        public RecipeSO recipeSO;
    }
    private float waitTimer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update() // will only be run by server
    {
        if (!IsServer) return;
        if(!GameManager.Instance.IsGamePlaying()) return;

        // Has not requested anything
        if (!HasRequestedKitchenObjectSO())
        {
            waitTimer += Time.deltaTime;
            if(waitTimer > waitTimerMax)
            {
                waitTimer = 0f;
                int count = recipeListSO.recipeSOList.Count;
                int recipeSOInex = UnityEngine.Random.Range(0, count);

                // Tell all the clients to request the recipe
                SetRequestedRecipeClientRpc(recipeSOInex);
                OnAnyKicthenObjectRequested?.Invoke(this, new OnKitchenObjectRequestedEventArgs // Going to deliveryManger
                {
                    recipeSO = requestedRecipeSO
                });
            }
        }
    }

    public RecipeSO GetRecipeSOForIndex(int index)
    {
        return recipeListSO.recipeSOList[index];
    }
    

    [ClientRpc]
    private void SetRequestedRecipeClientRpc(int index)
    {
        requestedRecipeSO = recipeListSO.recipeSOList[index];
        OnRecipeRequested?.Invoke(this, index);
    }

    public override void Interact(Player player)
    {
        Debug.Log("Interacting with customers");
        if (HasRequestedKitchenObjectSO())
        {
            if (player.HasKitchenObject())
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate))
                {
                    Debug.Log("Serving Customer");
                    ServeCustomer(plate); // synced
                    player.GetKitchenObject().DestroySelf(); // synced
                }
            }
            else
            {
                EmptyInteractionServerRpc();
            }
        }
    }

    private void ServeCustomer(PlateKitchenObject plate)
    {
        // Iterate through the recipe's kitchen objects
        foreach (KitchenObjectSO requestedRecipeKitchenObjectSO in requestedRecipeSO.KitchenObjectSOList)
        {
            bool foundKitchenObject = false;
            // Iterate through the plate's kitchen objects
            foreach (KitchenObjectSO kitchenObjectSO in plate.GetKitchenObjectSOList())
            {
                // If the plate contains the requested recipe's kitchen object
                if (requestedRecipeKitchenObjectSO == kitchenObjectSO)
                {
                    foundKitchenObject = true;
                    break;
                }
            }

            // If one KitchenObject was not in the plate, then the recipe is not complete
            if (!foundKitchenObject)
            {
                RecipeFailedServerRpc();
                return;
            }
        }
        // If there was no problem, then it is a success
        RecipeSuccessServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EmptyInteractionServerRpc()
    {
        EmptyInteractionClientRpc();
    }
    [ClientRpc] 
    private void EmptyInteractionClientRpc()
    {
        OnPlayerInteraction?.Invoke(this, new OnPlayerInteractionEventArgs()
        {
            interactionResult = InteractionResult.empty
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void RecipeFailedServerRpc()
    {
        OnAnyRecipeFailure?.Invoke(this, EventArgs.Empty);
        RecipeFailedClientRpc();
    }
    [ClientRpc]
    private void RecipeFailedClientRpc()
    {
        OnPlayerInteraction?.Invoke(this,new OnPlayerInteractionEventArgs()
        {
            interactionResult = InteractionResult.failure
        });
        ClearRequestedKitchenObjectSO();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RecipeSuccessServerRpc()
    {
        OnAnyRecipeSuccess?.Invoke(this, new OnRecipeDeliveryEventArgs()
        {
            recipeSO = requestedRecipeSO
        });
        RecipeSuccessClientRpc();
    }
    [ClientRpc]
    private void RecipeSuccessClientRpc()
    {
        OnPlayerInteraction?.Invoke(this, new OnPlayerInteractionEventArgs()
        {
            interactionResult = InteractionResult.success
        });
        ClearRequestedKitchenObjectSO();
    }
    private bool HasRequestedKitchenObjectSO()
    {
        return requestedRecipeSO != null;
    }
    private void ClearRequestedKitchenObjectSO()
    {
        requestedRecipeSO = null;
    }
}
