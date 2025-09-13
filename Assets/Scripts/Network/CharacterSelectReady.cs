using System;
using System.Collections.Generic;
using Unity.Netcode;

public class CharacterSelectReady : NetworkBehaviour
{
    public event EventHandler OnAnyPlayerReadyChanged;
    public static CharacterSelectReady Instance { get; private set; }
    private Dictionary<ulong, bool> playerReadyDictionary;
    private void Awake()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();
        Instance = this;
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        SetPlayerReadyClientRpc(senderClientId);
        playerReadyDictionary[senderClientId] = true;
        bool allplayersReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allplayersReady = false;
                break;
            }
        }

        if (allplayersReady)
        {
            GameLobby.Instance.DeleteLobby();
            Loader.NetworkLoad(Loader.Scene.GameScenePart1);
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyDictionary[clientId] = true;
        OnAnyPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
