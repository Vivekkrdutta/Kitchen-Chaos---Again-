using UnityEngine;

public class StoveCounterSounds : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    private AudioSource audioSource;
    bool playWarningSound = false;
    private float warningSoundTimer = 0f;
    private float warningSoundTimerMax = 0.2f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = 0.5f;
        playWarningSound = stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount;
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangeEventArgs e)
    {
        bool playSound = e.state == StoveCounter.State.Frying || StoveCounter.State.Fried == e.state;
        if (playSound)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    private void Update()
    {
        if (playWarningSound)
        {
            warningSoundTimer -= Time.deltaTime;
            if(warningSoundTimer < 0)
            {
                warningSoundTimer = warningSoundTimerMax;

                SoundManager.Instance.PlayWarningSound(stoveCounter.transform.position);
            }
        }
    }
}
