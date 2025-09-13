using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Netcode;
public class DeliveryManager : NetworkBehaviour
{
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailure;

    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSpawned;
    public static DeliveryManager Instance { get; private set; }
    [SerializeField] private RecipeListSO recipeListSO;
    [SerializeField] private bool useCustomerCounter; // Extra

    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer;
    private readonly float spawnRecipeTimerMax = 4f;
    private readonly int waitingRecipesMax = 4;
    private int succesfullRecipesNumber = 0;
    private void Awake()
    {
        waitingRecipeSOList = new List<RecipeSO>();
        Instance = this;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer || !useCustomerCounter) return; // Only the server will handle the logic if we are using the customer counter
        CustomerCounter.OnAnyKicthenObjectRequested += CustomerCounter_OnKicthenObjectRequested;
        CustomerCounter.OnAnyRecipeSuccess += CustomerCounter_OnRecipeSuccess;
        CustomerCounter.OnAnyRecipeFailure += CustomerCounter_OnRecipeFailure;
    }

    private void CustomerCounter_OnRecipeFailure(object sender, EventArgs e)
    {
        DeliverWrongRecipeServerRpc();
    }

    private void CustomerCounter_OnRecipeSuccess(object sender, CustomerCounter.OnRecipeDeliveryEventArgs e)
    {
        int removeRecipeSOIndex = waitingRecipeSOList.IndexOf(e.recipeSO);
        DeliverCorrectRecipeServerRpc(removeRecipeSOIndex);
    }

    private void CustomerCounter_OnKicthenObjectRequested(object sender, CustomerCounter.OnKitchenObjectRequestedEventArgs e)
    {
        int recipeSOIndex = recipeListSO.recipeSOList.IndexOf(e.recipeSO);
        if (recipeSOIndex >= 0 && recipeSOIndex < recipeListSO.recipeSOList.Count)
        {
            // Spawn new recipe for all clients
            SpawnNewRecipeClientRpc(recipeSOIndex);
        }
    }

    private void Update()
    {
        if (!IsServer || useCustomerCounter) { return; } // only the server will have the logic running

        spawnRecipeTimer += Time.deltaTime;
        if (spawnRecipeTimer > spawnRecipeTimerMax)
        {
            spawnRecipeTimer = 0f;
            if (GameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipesMax)
            {
                int randomRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
                SpawnNewRecipeClientRpc(randomRecipeSOIndex); // The host acts as both server and client. So, we need to call the client rpc for client's end only
            }
        }
    }

    [ClientRpc]
    private void SpawnNewRecipeClientRpc(int randomRecipeSOIndex)
    {
        RecipeSO recipeSO = recipeListSO.recipeSOList[randomRecipeSOIndex];
        waitingRecipeSOList.Add(recipeSO);
        OnRecipeSpawned?.Invoke(this, null);
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO recipeSO = waitingRecipeSOList[i];
            if (recipeSO.KitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentsMatchesRecipe = true;
                // quick check
                foreach (var recipeKitchenObjectSO in recipeSO.KitchenObjectSOList)
                {
                    bool foundIngredient = false;
                    // check if the plate contains this particular ingredient
                    foreach (var plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            // Ingredient does match
                            foundIngredient = true;
                            break;
                        }
                    }
                    if (!foundIngredient)
                    {
                        // some ingredient for the recipe was not found in the plate
                        plateContentsMatchesRecipe = false;
                        break;
                    }
                }
                if (plateContentsMatchesRecipe)
                {
                    // here lets make it client authorititive.
                    // The client will check if the recipe is correct or not and tell the server.
                    
                    int removeRecipeSOIndex = i;
                    DeliverCorrectRecipeServerRpc(removeRecipeSOIndex);

                    return;
                }
            }
        }
        DeliverWrongRecipeServerRpc(); // tell the server that the recipe delivered was wrong
    }
    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverWrongRecipeServerRpc()
    {
        DeliverWrongRecipeClientRpc(); // Tell all clients that a wrong delivery has been made
    }

    [ClientRpc]
    private void DeliverWrongRecipeClientRpc()
    {
        Debug.LogWarning("Wrong Delivery");
        OnRecipeFailure?.Invoke(this, null);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int removeRecipeSOIndex)
    {
        DeliverCorrectRecipeClientRpc(removeRecipeSOIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int removeRecipeSOIndex)
    {
        Debug.Log("Correct Delivery");
        waitingRecipeSOList.RemoveAt(removeRecipeSOIndex);

        OnRecipeCompleted?.Invoke(this, null);
        OnRecipeSuccess?.Invoke(this, null);
        succesfullRecipesNumber++;
    }
    public int GetSuccessfullRecipesAmount()
    {
        return succesfullRecipesNumber;
    }
}
