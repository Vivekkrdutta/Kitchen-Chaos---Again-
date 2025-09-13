using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect_DisconnectedUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        GameMultiplayer.Instance.OnPlayerDisconnected += GameMultiPlayer_OnPlayerDisconnected;
        Hide();
    }

    private void GameMultiPlayer_OnPlayerDisconnected(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDisconnected -= GameMultiPlayer_OnPlayerDisconnected;
    }
}
