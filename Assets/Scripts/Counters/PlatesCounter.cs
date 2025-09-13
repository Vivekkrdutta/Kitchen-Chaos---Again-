using System;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlatesRemoved;
    public event EventHandler OnPlatesSpawned;
    [SerializeField] private KitchenObjectSO plateKOSO;
    
    private float spawnTimer = 0;
    private float spawnTimerMax = 4f;
    private int spawnedAmount = 0;
    private int spawnedAmountMax = 4;

    private void Update()
    {
        if (!IsServer) return;

        spawnTimer += Time.deltaTime;
        if(spawnTimer > spawnTimerMax)
        {
            spawnTimer = 0;
            if(GameManager.Instance.IsGamePlaying() && spawnedAmount < spawnedAmountMax)
            {
                spawnedAmount++;

                SpawnPlateServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }

    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        OnPlatesSpawned?.Invoke(this, null);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemovePlateServerRpc()
    {
        if (spawnedAmount > 0)
        {
            spawnedAmount--;
            // Trigger event
            RemovePlateClientRpc();
        }
    }

    [ClientRpc]
    private void RemovePlateClientRpc()
    {
        OnPlatesRemoved?.Invoke(this, null);
    }

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            RemovePlateServerRpc();
            KitchenObject.SpawnKitchenObject(plateKOSO, player);
        }
    }
}
