using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button kickButton;
    [SerializeField] private TextMeshPro playerNameText;

    private void Start()
    {
        CharacterSelectReady.Instance.OnAnyPlayerReadyChanged += CharacterSelecteReady_OnAnyPlayerReadyChanged;
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiPlayer_OnPlayerDataNetworkListChanged;
        kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);
        if(kickButton.gameObject.activeSelf)
        {
            kickButton.onClick.AddListener(async () =>
            {
                PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataForIndex(playerIndex);

                if (playerData.clientId == NetworkManager.Singleton.LocalClientId) return;

                await GameLobby.Instance.KickPlayer(playerData.playerId.ToString());
                GameMultiplayer.Instance.KickOutPlayer(playerData.clientId);
            });
        }
        UpdateVisuals();
    }
    private void CharacterSelecteReady_OnAnyPlayerReadyChanged(object sender, EventArgs e)
    {
        UpdateVisuals();
    }

    private void GameMultiPlayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // for the gameobject itself
        if (GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex)){
            Show();
            
            // for the readygameobject ( the ready text )
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataForIndex(playerIndex);
            readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId));

            // for the playerName
            playerNameText.text = playerData.playerName.ToString();

            // Set the color
            if(playerData.colorId != -1)
            {
                playerVisual.SetColor(GameMultiplayer.Instance.GetColorForIndex(playerData.colorId));
            }
        }
        else{
            Hide();
        }
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
        CharacterSelectReady.Instance.OnAnyPlayerReadyChanged -= CharacterSelecteReady_OnAnyPlayerReadyChanged;
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiPlayer_OnPlayerDataNetworkListChanged;
    }
}
