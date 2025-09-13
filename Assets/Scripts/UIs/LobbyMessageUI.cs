using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;


    private void Start()
    {
        // we also need to unsubscribe this event as the publisher and the subscriber have different life times
        GameMultiplayer.Instance.OnFailedTojoinGame += GameMultiPlayer_OnFailedTojoinGame;
        GameLobby.Instance.OnCreateLobbyFailed += GameLobby_OnCreateLobbyFailed;
        GameLobby.Instance.OnJoiningFailed += GameLobby_OnJoiningFailed;
        GameLobby.Instance.OnQuickJoiningFailed += GameLobby_QuickJoiningFailed;

        Hide();
    }

    private void GameLobby_QuickJoiningFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Could not find a Lobby to join.");
    }

    private void GameLobby_OnJoiningFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to join selected lobby ! Ensure the code is correct");
    }

    private void GameLobby_OnCreateLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to create Lobby !!!");
    }

    private void GameMultiPlayer_OnFailedTojoinGame(object sender, System.EventArgs e)
    {
        ShowMessage(NetworkManager.Singleton.DisconnectReason);
        if(messageText.text == "")
        {
            messageText.text = "Failed to connect";
        }
    }

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    { 
        gameObject.SetActive(false); 
    }

    private void ShowMessage(string message)
    {
        Show();
        messageText.text = message;
    }
    private void OnDestroy()
    {
        // Unsubscribe
        GameMultiplayer.Instance.OnFailedTojoinGame -= GameMultiPlayer_OnFailedTojoinGame;
        GameLobby.Instance.OnCreateLobbyFailed -= GameLobby_OnCreateLobbyFailed;
        GameLobby.Instance.OnJoiningFailed -= GameLobby_OnJoiningFailed;
        GameLobby.Instance.OnQuickJoiningFailed -= GameLobby_QuickJoiningFailed;
    }

}
