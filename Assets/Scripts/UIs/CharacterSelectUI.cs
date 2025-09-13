using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(async () =>
        {
            NetworkManager.Singleton.Shutdown();
            await GameLobby.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        readyButton.onClick.AddListener(() => {
        
            CharacterSelectReady.Instance.SetPlayerReady();
        });
    }

    private void Start()
    {
        Lobby lobby = GameLobby.Instance.GetLobby();
        lobbyNameText.text = "Lobby Name: " + lobby.Name;
        lobbyCodeText.text = "Code: " + lobby.LobbyCode;
    }
}
