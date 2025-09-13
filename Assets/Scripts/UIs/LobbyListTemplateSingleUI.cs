using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListTemplateSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    private Lobby lobby;
    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
    }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (lobby == null) return;
            GameLobby.Instance.JoinWithId(lobby.Id); // here we cannot join by lobbyCode as when the lobbies are fetched via querrylobbiesasync,
            // the lobbyCode is not sent along with the lobbies ( i.e. lobby.lobbyCode = "", when querried )
        });
    }
}
