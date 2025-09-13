using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private Transform playerPrefab;

    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnPaused;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnPaused;
    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalPLayerReadyChanged;
    public static GameManager Instance {  get; private set; } 
    private enum State
    {
        waitingToStart,
        countDownToStart,
        gamePlaying,
        gameOver,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.waitingToStart);
    private NetworkVariable<float> countDownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 120f;
    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private bool isLocalPlayerReady = false;
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPausedDictionary;

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) 
        { 
            playerReadyDictionary = new Dictionary<ulong, bool>();  
            playerPausedDictionary = new Dictionary<ulong, bool>();
        }
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        // Handle pause/unpause event if the pauser is disconnected
        if (IsServer)
        {
            // Only server will invoke these
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += NetworkManager_SceneManager_OnLoadEventCompleted;
        }

    }

    // Method will be invoked once the server has loaded the GameScene for all the clients connected to it
    private void NetworkManager_SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if(sceneName == Loader.Scene.GameScenePart1.ToString())
        {
            Debug.Log("Load event Completed for part 1");
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Transform playerTransform = Instantiate(playerPrefab);
                playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }

            // Load part 2
            Loader.NetworkLoadAdditively(Loader.Scene.GameScenePart2);
        }

        else if (sceneName == Loader.Scene.GameScenePart2.ToString())
        {
            Debug.Log("Part 2 loaded");

            // Load part 3
            Loader.NetworkLoadAdditively(Loader.Scene.GameScenePart3);
        }
    }


    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        // Will check if the CONNECTED players are unpaused
        TestGamePaused();
    }
    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue) // Runs locally on the clients
    {
        if (isGamePaused.Value)
        {
            //Debug.Log("Pausing game 2");
            Time.timeScale = 0;
            OnMultiplayerGamePaused?.Invoke(this, null);
        }
        else
        {
            Time.timeScale = 1;
            OnMultiplayerGameUnPaused?.Invoke(this, null);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, null);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if(state.Value == State.waitingToStart)
        {
            isLocalPlayerReady = true;
            OnLocalPLayerReadyChanged?.Invoke(this, null); // Tell the local WaitingForOthersToStart that the local player is ready
            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default){

        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        if (!playerReadyDictionary.ContainsKey(senderClientId)){
            playerReadyDictionary[senderClientId] = true;
        }
        bool allplayersReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            if(!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]){
                allplayersReady = false;
                break;
            }
        }

        if (allplayersReady){
            Debug.Log("All players ready");
            state.Value = State.countDownToStart;
        }
    }

    private void Update()
    {
        if (!IsServer) return; // Run on server only

        switch (state.Value)
        {
            case State.waitingToStart:

                break;
            case State.countDownToStart:

                countDownToStartTimer.Value -= Time.deltaTime;
                if(countDownToStartTimer.Value < 0)
                {
                    state.Value = State.gamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;

                }
                break;
            case State.gamePlaying:

                gamePlayingTimer.Value -= Time.deltaTime;
                if(gamePlayingTimer.Value < 0)
                {
                    state.Value = State.gameOver;
                }
                break;
            case State.gameOver:
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return state.Value == State.gamePlaying;
    }
    public bool IsCountDownToStartActive()
    {
        return state.Value == State.countDownToStart;
    }
    public float GetCountDownToStartTimer()
    {
        return countDownToStartTimer.Value;
    }
    public bool IsGameOver()
    {
        return state.Value == State.gameOver;
    }

    public bool IsWaitingToStart()
    {
        return state.Value == State.waitingToStart;
    }   

    public float GetGamePlayingTimerNormalized()
    {
        return gamePlayingTimer.Value / gamePlayingTimerMax;
    }

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }
    public void TogglePauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused)
        {
            PauseGameServerRpc();
            OnLocalGamePaused?.Invoke(this, null);
        }
        else
        {
            UnPauseGameServerRpc();
            OnLocalGameUnPaused?.Invoke(this, null);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;
        TestGamePaused();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnPauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;
        TestGamePaused();
    }

    // We are not using a client Rpc to unpause/pause all clients locally. Instead, modifying isGamePaused will automatically do this
    private void TestGamePaused() 
    {
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId])
            {
                isGamePaused.Value = true;
                return;
            }
        }

        isGamePaused.Value = false;
    }
}
