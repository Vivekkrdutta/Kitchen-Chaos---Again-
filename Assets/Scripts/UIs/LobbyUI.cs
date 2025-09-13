using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinLobbyByCode;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    [SerializeField] private LobbyCreateUI lobbyCreateUI;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(async () =>
        {
            await GameLobby.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        createLobbyButton.onClick.AddListener(() => 
        {
            lobbyCreateUI.Show();
        });

        quickJoinButton.onClick.AddListener(() => {

            GameLobby.Instance.QuickJoinLobby();
        });

        joinLobbyByCode.onClick.AddListener(() => { 
        
            GameLobby.Instance.JoinWithCode(lobbyCodeInputField.text);
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerNameInputField.text = GameMultiplayer.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((value) =>
        {
            GameMultiplayer.Instance.SetPlayerName(value);
        });

        GameLobby.Instance.OnLobbyListChanged += GameLobby_OnLobbyListChanged;

        UpdateLobbyList(new List<Lobby>());
    }

    private void GameLobby_OnLobbyListChanged(object sender, GameLobby.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach(Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate,lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListTemplateSingleUI>().SetLobby(lobby); // set the lobby associated with the template(button)
        }
    }

    private void OnDestroy()
    {
        GameLobby.Instance.OnLobbyListChanged -= GameLobby_OnLobbyListChanged;
    }
}
