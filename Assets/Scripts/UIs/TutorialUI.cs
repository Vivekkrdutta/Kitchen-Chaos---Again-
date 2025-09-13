using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyMoveUpText;
    [SerializeField] private TextMeshProUGUI keyMoveDownText;
    [SerializeField] private TextMeshProUGUI keyMoveRightText;
    [SerializeField] private TextMeshProUGUI keyMoveLeftText;
    [SerializeField] private TextMeshProUGUI keyInteractText;
    [SerializeField] private TextMeshProUGUI keyInteractAltText;
    [SerializeField] private TextMeshProUGUI keyPauseText;

    private void Start()
    {
        UpdateVisual();
        GameInput.Instance.OnBindingRebind += GameInput_OnBindingRebind;
        GameManager.Instance.OnLocalPLayerReadyChanged += GameManager_OnLocalPLayerReadyChanged;
    }

    private void GameManager_OnLocalPLayerReadyChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsLocalPlayerReady())
        {
            Hide();
        }
    }

    private void GameInput_OnBindingRebind(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        keyMoveUpText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Move_up);
        keyMoveDownText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Move_Down);
        keyMoveRightText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Move_Right);
        keyMoveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Move_left);
        keyInteractText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Interact);
        keyInteractAltText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Interact_Alt);
        keyPauseText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Pause);
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
