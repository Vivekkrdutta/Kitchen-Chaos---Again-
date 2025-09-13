using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button resumeButton;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
            NetworkManager.Singleton.Shutdown();
        });
        resumeButton.onClick.AddListener(() => 
        {
            GameManager.Instance.TogglePauseGame();
        });
        optionsButton.onClick.AddListener(() =>
        {
            OptionsUI.Instance.Show();
        });
    }
    private void Start()
    {
        GameManager.Instance.OnLocalGamePaused += GameManager_OnLocalGamePaused;
        GameManager.Instance.OnLocalGameUnPaused += GameManager_OnLocalGameUnPaused;
        Hide();
    }

    private void GameManager_OnLocalGameUnPaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnLocalGamePaused(object sender, System.EventArgs e)
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
}
