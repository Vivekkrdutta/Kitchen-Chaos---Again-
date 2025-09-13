using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryResultUI : MonoBehaviour
{
    protected const string POPUPSTRING = "PopUp";
    [SerializeField] protected Image backGroundImage;
    [SerializeField] protected Image iconImage;
    [SerializeField] protected TextMeshProUGUI messageText;

    [SerializeField] protected Color successColor;
    [SerializeField] protected Color failedColor;
    [SerializeField] protected Sprite successSprite;
    [SerializeField] protected Sprite failedSprite;

    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }
    protected virtual void Start()
    {
        DeliveryManager.Instance.OnRecipeFailure += Delivery_OnRecipeFailed;
        DeliveryManager.Instance.OnRecipeSuccess += Delivery_OnRecipeSuccess;
        Hide();
    }

    protected virtual void Delivery_OnRecipeFailed(object sender, System.EventArgs e)
    {
        Show();
        backGroundImage.color = failedColor;
        messageText.text = "DELIVRY\nFAILED";
        iconImage.sprite = failedSprite;
        animator.SetTrigger(POPUPSTRING);
    }

    protected virtual void Delivery_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        Show();
        backGroundImage.color = successColor;
        messageText.text = "DELIVRY\nSUCCESS";
        iconImage.sprite = successSprite;
        animator.SetTrigger(POPUPSTRING);
    }

    protected void Show()
    {
        gameObject.SetActive(true);
    }
    protected void Hide()
    {
        gameObject.SetActive(false);
    }
}
