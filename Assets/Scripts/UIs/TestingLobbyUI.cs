using UnityEngine;
using UnityEngine.UI;

public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;

    private void Awake()
    {
        createGameButton.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.StartHost();
            Loader.NetworkLoad(Loader.Scene.CharacterSelectScene);
        });
        joinGameButton.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.StartClient();
        });
    }
}
