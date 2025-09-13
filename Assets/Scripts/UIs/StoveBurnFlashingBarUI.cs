using UnityEngine;

public class StoveBurnFlashingBarUI : MonoBehaviour
{
    private const string IS_FLASHING = "IsFlashing";
    [SerializeField] private StoveCounter stoveCounter;
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        animator.SetBool(IS_FLASHING, false);
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = 0.5f;
        bool flashing = stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount;
        if (flashing)
        {
            animator.SetBool(name: IS_FLASHING, value: true);
        }
    }

    private void OnDisable()
    {
        animator.SetBool(IS_FLASHING, false);
    }
}
