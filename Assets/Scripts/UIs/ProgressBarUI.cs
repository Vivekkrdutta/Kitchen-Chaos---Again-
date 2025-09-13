using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    private const string BURNINGSTRING = "Burning";

    [SerializeField] private GameObject gameObjectHavingProgressBar;
    [SerializeField] private Image barImage;

    private IHasProgress iHasProgress;

    private void Start()
    {
        iHasProgress = gameObjectHavingProgressBar.GetComponent<IHasProgress>();
        if(iHasProgress == null)
        {
            Debug.LogError("The object does not have a process bar");
        }

        iHasProgress.OnProgressChanged += IHasProgress_OnProgressChanged;
        barImage.fillAmount = 0;
        Hide();
    }

    private void IHasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        barImage.fillAmount = e.progressNormalized;
        if (barImage.fillAmount == 0 || barImage.fillAmount == 1)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void Hide()
    {
        gameObject.SetActive(value:false);
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
}
