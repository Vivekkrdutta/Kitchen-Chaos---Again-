using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMultiplayer : NetworkBehaviour
{
    private const string PLAYERFREFS_PLAYERNAMEMULTIPLAYER = "PlayerNameMultiplayer";

    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler OnTryingTojoinGame;
    public event EventHandler OnFailedTojoinGame;
    public event EventHandler OnPlayerDisconnected;

    [SerializeField] private int maxPlayersAmount = 4;
    [SerializeField] private KitchenObjeSOListSO KitchenObjeSOListSO;
    [SerializeField] private List<Color> playerColors = new List<Color>();

    private string playerName;

    public int GetMaxPlayers()
    {
        return maxPlayersAmount;
    }

    private NetworkList<PlayerData> playerDataNetworkList;
    public static GameMultiplayer Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerName = PlayerPrefs.GetString(PLAYERFREFS_PLAYERNAMEMULTIPLAYER,"Player" + (UnityEngine.Random.Range(0,1000)).ToString());

        playerDataNetworkList = new NetworkList<PlayerData>(writePerm:NetworkVariableWritePermission.Server); // It has to be initialized here only
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    public string GetPlayerName()
    {
        Debug.Log("Getting playername " + playerName);
        return playerName;
    }

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName; // It havs the value event before the server starts
        PlayerPrefs.SetString(PLAYERFREFS_PLAYERNAMEMULTIPLAYER, playerName);
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, null);
    }

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        base.OnNetworkSpawn();
    }
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for(int i = 0; i< playerDataNetworkList.Count; i++)
        {
            if(playerDataNetworkList[i].clientId == clientId)
            {
                // Remove the player data from the list. All listeners listening to it will work now.
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    public void StartClient()
    {
        OnTryingTojoinGame?.Invoke(this, null);
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName,ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerName = playerName;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerId = playerId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData { 
            
            clientId = clientId,
            colorId = 9,
        });

        PlayerData playerData = new PlayerData
        {
            clientId = clientId,
            colorId = GetFirstUnusedColorId(),
        };


        playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)] = playerData;
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if(SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            response.Approved = false;
            response.Reason = "Game has Already Started";
            Debug.Log("Denying the join request of " + request.ClientNetworkId);
            return;
        }
        if(NetworkManager.Singleton.ConnectedClientsIds.Count >= maxPlayersAmount)
        {
            response.Approved = false;
            response.Reason = "Game is full";
            Debug.Log("Denying the join request of " + request.ClientNetworkId);
            return;
        }
        response.Approved = true;
        response.CreatePlayerObject = true;
    }


    private void NetworkManager_Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log("Failed to join");
        OnFailedTojoinGame?.Invoke(this, null);
        OnPlayerDisconnected?.Invoke(this, null);
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSOType, IKitchenObjectParent kitchenObjectParent)
    {
        SpawnKitchenObjectServerRpc(GetKitchenObjectSOIndexFromList(kitchenObjectSOType), kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kOParentNOReference)
    {
        if(kOParentNOReference.TryGet(out NetworkObject parentNetworkObject))
        {
            IKitchenObjectParent kitchenObjectParent = parentNetworkObject.GetComponent<IKitchenObjectParent>();

            if (kitchenObjectParent.HasKitchenObject()) return;

            Transform kitchenObjectTransform = Instantiate(GetKitchenObjectSOFromIndex(kitchenObjectSOIndex).prefab);

            KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

            NetworkObject kitchenNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();

            kitchenNetworkObject.Spawn(true);

            kitchenObject.SetKitchenObjectParent(kitchenObjectParent); // Try to make the parent locally
        }
    }

    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        if(kitchenObject.TryGetComponent(out NetworkObject kitchenNetworkObject))
        {
            DestroyKitchenObjectServerRpc(kitchenNetworkObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenbjectNetworkObjectReference)
    {
        if(kitchenbjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject))
        {
            if(kitchenObjectNetworkObject.TryGetComponent(out KitchenObject kitchenObject))
            {
                IKitchenObjectParent kitchenObjectParent = kitchenObject.GetKitchenObjectParent();
                DestroyKitchenObjectClientRpc(kitchenObjectParent.GetNetworkObject());
                kitchenObjectNetworkObject.Despawn(true);
            }
        }
    }

    [ClientRpc]
    private void DestroyKitchenObjectClientRpc(NetworkObjectReference parentNetworkObjectReference)
    {
        if(parentNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            IKitchenObjectParent kitchenObjectParent = playerNetworkObject.GetComponent<IKitchenObjectParent>();
            kitchenObjectParent.ClearKitchenObject();
        }
    }

    public int GetKitchenObjectSOIndexFromList(KitchenObjectSO kitchenObjectSO)
    {
        return KitchenObjeSOListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    public KitchenObjectSO GetKitchenObjectSOFromIndex(int index)
    {
        return KitchenObjeSOListSO.kitchenObjectSOList[index];
    }

    public bool IsPlayerIndexConnected(int index)
    {
        return index < playerDataNetworkList.Count;
    }
    public Color GetColorForIndex(int index)
    {
        return playerColors[index];
    }

    public PlayerData GetPlayerDataForIndex(int index)
    {
        return playerDataNetworkList[index];
    }
    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }
    public void UpdatePlayerColor(int colorId)
    {
        // Requested by the local playervisual to update the color on behalf the of the local player
        UpdatePlayerColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerColorServerRpc(int colorId,ServerRpcParams serverRpcParams = default)
    {
        // we get the sender's color id now, check if its already in use
        if (!IsColorAvailable(colorId)) { return; }

        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        if(playerDataIndex != -1)
        {
            PlayerData playerData = GetPlayerDataForIndex(playerDataIndex);
            playerData.colorId = colorId;
            playerDataNetworkList[playerDataIndex] = playerData;
        }
    }

    private bool IsColorAvailable(int colorId)
    {
        foreach(PlayerData playerData in playerDataNetworkList)
        {
            if(playerData.colorId == colorId)
            {
                return false;
            }
        }
        return true;
    }
    private int GetFirstUnusedColorId()
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }

    private void RemovePlayerDataUsingClientId(ulong clientId)
    {
        for(int i = 0;i < playerDataNetworkList.Count; i++)
        {
            if(clientId == playerDataNetworkList[i].clientId)
            {
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    public void KickOutPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        RemovePlayerDataUsingClientId(clientId);
    }
}
