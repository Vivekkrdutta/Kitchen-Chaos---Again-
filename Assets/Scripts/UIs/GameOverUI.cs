using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button playAgainButton;
    [SerializeField] private TextMeshProUGUI recipesDeliveredText;
    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        playAgainButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
            NetworkManager.Singleton.Shutdown();
        });
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver())
        {
            Show();
            recipesDeliveredText.text = DeliveryManager.Instance.GetSuccessfullRecipesAmount().ToString();
        }
        else
        {
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
}
