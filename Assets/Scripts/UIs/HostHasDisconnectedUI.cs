using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostHasDisconnectedUI : MonoBehaviour
{
    [SerializeField] Transform disconnectionVisualsTransform;
    [SerializeField] private Button playAgainButton;

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectedCallback;
        playAgainButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
            NetworkManager.Singleton.Shutdown();
        });
        HideVisuals();
        Hide();
    }

    private void NetworkManager_OnClientDisconnectedCallback(ulong clientId)
    {
        if(clientId == NetworkManager.Singleton.LocalClientId) // method returns the clientId of the local manager as there is no server
        {
            Show();
            // Check after the end of the current frame
            StartCoroutine(ServerConnectivityCheckNumerator());
        }
    }

    private void ShowVisuals()
    {
        disconnectionVisualsTransform.gameObject.SetActive(true);
    }
    private void HideVisuals()
    {
        disconnectionVisualsTransform.gameObject.SetActive(false);
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator ServerConnectivityCheckNumerator()
    {
        int count = 500;
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (!IsServerConnected() && disconnectionVisualsTransform != null){
                ShowVisuals();
                break;
            }
            count--;
            if(count < 0)
            {
                HideVisuals();
                Hide();
                break;
            }
        }     
    }

    private bool IsServerConnected()
    {
        return NetworkManager.Singleton.IsConnectedClient;
    }

    private void OnDestroy()
    {
        if(NetworkManager.Singleton != null)
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectedCallback;
    }

}
