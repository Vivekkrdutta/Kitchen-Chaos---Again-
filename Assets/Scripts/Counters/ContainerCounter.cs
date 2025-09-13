using System;
using Unity.Netcode;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPlayerGrabbedObject;
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            // Create a new one and Assign it to the player
            //KitchenObject.SpawnKitchenObject(kitchenObjectSO, player); // [ Deprecated ]
            GameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO,player);

            InteractionLogicServerRpc();
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void InteractionLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        OnPlayerGrabbedObject?.Invoke(this, null);
    }
}
