using System;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter,IHasProgress
{
    public event EventHandler<OnStateChangeEventArgs> OnStateChanged;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public class OnStateChangeEventArgs : EventArgs
    {
        public State state;
    }

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOs;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOs;

    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void FryingTimer_OnValueChanged(float prevValue,float newValue)
    {
        float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimeMax : 1f;
        // Progress Changed Event publish

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }

    private void BurningTimer_OnValueChanged(float prevValue,float newValue)
    {
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;
        // Progress changed:

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }

    private void State_OnValueChanged(State prevValue,State newValue)
    {
        // Invoke the onstatechagned event :
        OnStateChanged?.Invoke(this, new OnStateChangeEventArgs()
        {
            state = newValue
        });

        if (newValue == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
            {
                progressNormalized = 0f
            });
        }
        else if (newValue == State.Burned)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
            {
                progressNormalized = 1f
            });
        }
    }

    private void Update()
    {
        if (!IsServer) return; // Only server will handle the stove counter updates

        if (HasKitchenObject())
        {
            // State Machine
            switch (state.Value)
            {
                case State.Idle:

                    break;
                case State.Frying:

                    fryingTimer.Value += Time.deltaTime;

                    if (fryingTimer.Value > fryingRecipeSO.fryingTimeMax)
                    {
                        GetKitchenObject().DestroySelf();

                        // Now spawn the output
                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                        state.Value = State.Fried;
                        burningTimer.Value = 0;

                        // Set the burning recipe for all the clients
                        SetBurningRecipeSOClientRpc(

                            GameMultiplayer.Instance.GetKitchenObjectSOIndexFromList(GetKitchenObject().GetKitchenObjectSO())
                        );
                    }
                    break;
                case State.Fried: // Burning

                    burningTimer.Value += Time.deltaTime;

                    if (burningTimer.Value > burningRecipeSO.burningTimerMax)
                    {
                        GetKitchenObject().DestroySelf();

                        // Now spawn the output
                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);
                        state.Value = State.Burned;
                    }
                    break;
                case State.Burned:
                
                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject()) // Does not have it
        {
            if (player.HasKitchenObject())
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    // Get the kitchenObject
                    KitchenObject kitchenObject = player.GetKitchenObject();

                    // Get the index
                    int kitchenObjectSOIndex = 
                    GameMultiplayer.Instance.GetKitchenObjectSOIndexFromList(kitchenObject.GetKitchenObjectSO());
                    
                    // Set this as the player
                    kitchenObject.SetKitchenObjectParent(this);

                    // Inform the Server about the update:
                    InteractLogicPlaceObjectOnCounterServerRpc(kitchenObjectSOIndex);
                }
                else
                {
                    Debug.LogWarning("Cannot Place it");
                }
            }
        }
        else // It has the KitchenObject
        {
            if (!player.HasKitchenObject())
            {
                // Give it to the player
                GetKitchenObject().SetKitchenObjectParent(player);
                // Inform the server about the situation
                SetStateIdleServerRpc();
            }
            else // It means player has something
            {
                // Check if player has a plate
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate))
                {
                    // Check if the counter's KO can be added to the plate
                    if (plate.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        SetStateIdleServerRpc();
                    }
                }
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }

    public bool IsFried()
    {
        return state.Value == State.Fried;
    }
    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        return GetFryingRecipeSOForInput(inputKitchenObjectSO) != null;
    }

    private FryingRecipeSO GetFryingRecipeSOForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOs)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOs)
        {
            if (burningRecipeSO.input == inputKitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }

    // Server Rpcs :

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenOjectSOIndex)
    {
        fryingTimer.Value = 0;

        // Change the state
        state.Value = State.Frying;
        SetFryingRecipeSOClientRpc(kitchenOjectSOIndex);
    }

    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        // Assign the fryingRecipeSO for all the clients ( very important )
        fryingRecipeSO = GetFryingRecipeSOForInput(GameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex));
    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        // Assign the fryingRecipeSO for all clients
        burningRecipeSO = GetBurningRecipeSOForInput(GetKitchenObject().GetKitchenObjectSO());
        Debug.Log( burningRecipeSO == null);
    }
}
