using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    private void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.StartHost();
            Hide();
        });
        startClientButton.onClick.AddListener(() =>
        {
           GameMultiplayer.Instance.StartClient();
            Hide();
        });
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
