using UnityEngine;

public class WaitingForOthersToStart : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnLocalPLayerReadyChanged += GameManager_OnLocalPLayerReadyChanged;
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        //Debug.Log("State Changed ");
        if (GameManager.Instance.IsCountDownToStartActive())
        {
            Hide();
        }
    }

    private void GameManager_OnLocalPLayerReadyChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsLocalPlayerReady())
        {
            Show();
        }
    }

    private void Show()
    {
        //Debug.Log("Showing");
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        //Debug.Log("Hiding");
        gameObject.SetActive(false);
    }
}
