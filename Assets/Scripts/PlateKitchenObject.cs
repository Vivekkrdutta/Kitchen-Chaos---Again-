using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectsSOs;
    private List<KitchenObjectSO> kitchenObjectSOs;
    protected override void Awake()
    {
        base.Awake();
        kitchenObjectSOs = new List<KitchenObjectSO>();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectsSOs.Contains(kitchenObjectSO)) return false;

        if (kitchenObjectSOs.Contains(kitchenObjectSO)) return false;

        int kitchenObjectIndex = GameMultiplayer.Instance.GetKitchenObjectSOIndexFromList(kitchenObjectSO);

        // Tell the server
        AddIngredientServerRpc(kitchenObjectIndex);
        return true;
    }
    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = GameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        kitchenObjectSOs.Add(kitchenObjectSO);
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs()
        {
            kitchenObjectSO = kitchenObjectSO
        });
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOs;
    }
}
