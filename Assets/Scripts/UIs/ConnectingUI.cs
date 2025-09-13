using System;
using TMPro;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private string connectingString = "Connecting";
    [SerializeField] private TextMeshProUGUI connectingText;
    private float connectionTimer = 0f;
    private float connectionRoundTimer = 3f;
    private void Start()
    {
        GameMultiplayer.Instance.OnTryingTojoinGame += GameMultiPlayer_OnTryingTojoinGame;
        GameMultiplayer.Instance.OnFailedTojoinGame += GameMultiPlayer_OnFailedTojoinGame;
        GameLobby.Instance.OnCreateLobbyStarted += GameLobby_OnCreateLobbyStarted;
        GameLobby.Instance.OnJoiningStarted += GameLobby_OnJoiningStarted;

        GameLobby.Instance.OnCreateLobbyFailed += GameLobby_OnCreateLobbyFailed;
        GameLobby.Instance.OnJoiningFailed += GameLobby_OnJoiningFailed;
        GameLobby.Instance.OnQuickJoiningFailed += GameLobby_QuickJoiningFailed;

        Hide();
    }

    private void GameLobby_QuickJoiningFailed(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameLobby_OnJoiningFailed(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameLobby_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameLobby_OnJoiningStarted(object sender, System.EventArgs e)
    {
        connectingText.text = "Joining Lobby...";
        Show();
    }

    private void GameLobby_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        connectingText.text = "Creating lobby...";
        Show();
    }

    private void GameMultiPlayer_OnFailedTojoinGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameMultiPlayer_OnTryingTojoinGame(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnTryingTojoinGame -= GameMultiPlayer_OnTryingTojoinGame;
        GameMultiplayer.Instance.OnFailedTojoinGame -= GameMultiPlayer_OnFailedTojoinGame;
    }
}
