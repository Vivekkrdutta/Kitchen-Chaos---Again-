using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanUP : MonoBehaviour
{
    private void Start()
    {
        if(NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if(GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }
        if(GameLobby.Instance != null)
        {
            Destroy(GameLobby.Instance.gameObject);
        }
    }
}
