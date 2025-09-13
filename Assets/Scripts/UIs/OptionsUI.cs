using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameInput;
public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance {  get; private set; }
    [SerializeField] private Button soundEffectsButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI soundEffectsText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private Transform pressToRebindTransform;


    [Header("Enter all the control buttons here")]
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button interactButton;
    [SerializeField] private Button interactAltButton;
    [SerializeField] private Button pauseButton;

    [Header("Enter all the control texts here")]
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI interactAlternateText;
    [SerializeField] private TextMeshProUGUI pauseText;

    private void Awake()
    {
        Instance = this;
        soundEffectsButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeSound();
            UpdateVisuals();
        });
        musicButton.onClick.AddListener(() => 
        {
            MusicManager.Instance.ChangeSound();
            UpdateVisuals();
        });
        closeButton.onClick.AddListener(() => {

            Hide();
        });

        moveUpButton.onClick.AddListener(() => RebindBinding(Bindings.Move_up));
        moveDownButton.onClick.AddListener(() => RebindBinding(Bindings.Move_Down));
        moveRightButton.onClick.AddListener(() => RebindBinding(Bindings.Move_Right));
        moveLeftButton.onClick.AddListener(() => RebindBinding(Bindings.Move_left));
        interactButton.onClick.AddListener(() => RebindBinding(Bindings.Interact));
        interactAltButton.onClick.AddListener(() => RebindBinding(Bindings.Interact_Alt));
        pauseButton.onClick.AddListener(() => RebindBinding(Bindings.Pause)); 

    }

    private void Start()
    {
        GameManager.Instance.OnLocalGameUnPaused += GameManager_OnGameUnPaused;
        UpdateVisuals();
        Hide();
        HidePressToRebindKey();
    }

    private void GameManager_OnGameUnPaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void UpdateVisuals()
    {
        soundEffectsText.text = "Sound Effects : " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
        musicText.text = "Music : " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);

        // Update the Control keys
        moveUpText.text = GameInput.Instance.GetBindingText(Bindings.Move_up);
        moveDownText.text = GameInput.Instance.GetBindingText(Bindings.Move_Down);
        moveRightText.text = GameInput.Instance.GetBindingText(Bindings.Move_Right);
        moveLeftText.text = GameInput.Instance.GetBindingText(Bindings.Move_left);
        interactText.text = GameInput.Instance.GetBindingText(Bindings.Interact);
        interactAlternateText.text = GameInput.Instance.GetBindingText(Bindings.Interact_Alt);
        pauseText.text = GameInput.Instance.GetBindingText(Bindings.Pause);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void ShowPressToRebindKey()
    {
        pressToRebindTransform.gameObject.SetActive(true);
    }
    public void HidePressToRebindKey()
    {
        pressToRebindTransform.gameObject.SetActive(false);
    }

    private void RebindBinding(Bindings binding)
    {
        ShowPressToRebindKey();
        GameInput.Instance.RebindBindings(binding, () =>
        {
            HidePressToRebindKey();
            UpdateVisuals();
        });
    }
}
