using TMPro;
using UnityEngine;

public class GameStartCountDownUI : MonoBehaviour
{
    private const string NUMBERPOPUPSTRING = "NumberPopUp";
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI countDownText;
    private int previousCountDownNumber;
    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide(); 
    }

    private void Update()
    {
        int countDownNumber = Mathf.CeilToInt(GameManager.Instance.GetCountDownToStartTimer());
        countDownText.text = countDownNumber.ToString();
        if(countDownNumber != previousCountDownNumber)
        {
            previousCountDownNumber = countDownNumber;
            animator.SetTrigger(NUMBERPOPUPSTRING);
            SoundManager.Instance.PlayCountDownSound();
        }

    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsCountDownToStartActive())
        {
            Show();
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
