using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] protected Transform[] visualGameObjects;

    private bool showing = false;
    protected virtual void Start()
    {
        if(Player.LocalInstance != null)
        {
            Debug.Log("Local Player is not null");
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
    }
    
    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        if(Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if(e.selectedCounter == baseCounter)
        {
            showing = true;
            Show();
        }
        else if (showing)
        {
            showing = false;
            Hide();
        }
    }

    protected virtual void Show()
    {
        foreach (Transform t in visualGameObjects)
        {
            t.gameObject.SetActive(true);
        }
    }
    protected virtual void Hide()
    {
        foreach(Transform t in visualGameObjects)
        {
            t.gameObject.SetActive(false);
        }
    }
}
