using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLobby : MonoBehaviour
{

    private const string _KEYRELAYJOINCODE = "RelayJoinCode001";

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoiningStarted;
    public event EventHandler OnQuickJoiningFailed;
    public event EventHandler OnJoiningFailed;

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs: EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public static GameLobby Instance { get; private set; }
    private Lobby joinedLobby;

    private float heartBeatTimer = 0f;
    private float listLobbiesTimer = 0f;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    private void Update()
    {
        HandleHeartBeat();
        HandlePeriodicListLobbies();
    }

    private void HandlePeriodicListLobbies()
    {
        // only if not joined and signedin and at the lobby scenea at current
        if(joinedLobby == null && AuthenticationService.Instance.IsSignedIn && SceneManager.GetActiveScene().name == Loader.Scene.LobbyScene.ToString()) // Only if the player has not yet joined any lobby
        {
            listLobbiesTimer -= Time.deltaTime;
            if (listLobbiesTimer < 0)
            {
                float listLObbiesTimerMax = 3f;
                listLobbiesTimer = listLObbiesTimerMax;
                ListLobbies();
            }
        }
    }

    private async void HandleHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;
            if(heartBeatTimer < 0f)
            {
                float heartBeatTimerMax = 5f;
                heartBeatTimer = heartBeatTimerMax;

                Debug.Log("Sending heartbeat");
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                }
                catch(LobbyServiceException e)
                {
                    Debug.Log(e.Message);
                }

            }
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    public Lobby GetLobby()
    {
        return joinedLobby;
    }

    // Authentication for Using the Unity's Gaming services
    private async void InitializeUnityAuthentication()
    {
        if(UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new();
            initializationOptions.SetProfile("RandomPlayer" + ((int)UnityEngine.Random.Range(0,10f)).ToString());

            await UnityServices.InitializeAsync(initializationOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation =  await RelayService.Instance.CreateAllocationAsync(GameMultiplayer.Instance.GetMaxPlayers());
            return allocation;
        }catch (RelayServiceException cse)
        {
            Debug.Log(cse.Message);
            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        } catch (RelayServiceException cse)
        {
            Debug.Log(cse.Message);
            return default;
        }
    }

    public async void CreateLobby(string lobbyName,bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, null);
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, GameMultiplayer.Instance.GetMaxPlayers(), new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);

            
            
            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {_KEYRELAYJOINCODE,new DataObject(DataObject.VisibilityOptions.Member,relayJoinCode) }
                }
            });

            
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "wss"));

            GameMultiplayer.Instance.StartHost();

            Loader.NetworkLoad(Loader.Scene.CharacterSelectScene);
        }
        catch(LobbyServiceException e)
        {
            OnCreateLobbyFailed?.Invoke(this, null);
            Debug.Log(e.Message);
        }
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e.Message);
            return default;
        }
    }

    public async void QuickJoinLobby()
    {
        OnJoiningStarted?.Invoke(this, null);
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = joinedLobby.Data[_KEYRELAYJOINCODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "wss"));

            GameMultiplayer.Instance.StartClient();
        }
        catch(LobbyServiceException e)
        {
            OnQuickJoiningFailed?.Invoke(this, null);
            Debug.Log(e.Message);
        }

    }

    public async void JoinWithCode(string code)
    {
        if (code == "") return;
        OnJoiningStarted?.Invoke(this,null);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);

            string relayJoinCode = joinedLobby.Data[_KEYRELAYJOINCODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "wss"));

            GameMultiplayer.Instance.StartClient();
        }
        catch(LobbyServiceException e)
        {
            OnJoiningFailed?.Invoke(this, null);
            Debug.Log(e.Message);
        }
    }

    public async void JoinWithId(string lobbyId)
    {
        OnJoiningStarted?.Invoke(this, null);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            string relayJoinCode = joinedLobby.Data[_KEYRELAYJOINCODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "wss"));

            GameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            OnJoiningFailed?.Invoke(this, null);
            Debug.Log(e.Message);
        }
    }

    public async void DeleteLobby()
    {
        if(joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                joinedLobby = null;
            }
            catch(LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    public async Task LeaveLobby()
    {
        if(joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id,AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
            }
            catch(LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    public async Task KickPlayer(string playerId) // Only used by the Host
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter> // Add a filter to show only those which have available slots > 0
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0",QueryFilter.OpOptions.GT),
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                lobbyList = queryResponse.Results,
            });
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }

    }
}
