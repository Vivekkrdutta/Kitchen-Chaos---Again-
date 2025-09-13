using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const string PLAYER_PREFS_SOUNDEFFECTS_VOLUME = "SoundEffectsVolume";
    private float volume = 1f;
    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioClipsRefsSO audioClipsRefsSO;

    private void Awake()
    {
        Instance = this;
        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUNDEFFECTS_VOLUME, 1f);
    }
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailure += DeliveryManager_OnRecipeFailure;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        //Player.Instance.OnPickedUpSomething += Player_OnPickedUpSomething;
        Player.OnAnyPlayerPickedUpSomething += Player_OnAnyPlayerPickedUpSomething;
        BaseCounter.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    }

    private void Player_OnAnyPlayerPickedUpSomething(object sender, System.EventArgs e)
    {
        PlaySound(audioClipsRefsSO.objectPickup, (sender as Player).transform.position);
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e)
    {
        PlaySound(audioClipsRefsSO.trash,(sender as TrashCounter).transform.position);
    }

    private void BaseCounter_OnAnyObjectPlacedHere(object sender, System.EventArgs e)
    {
        PlaySound(audioClipsRefsSO.objectDrop,(sender as BaseCounter).transform.position);
    }

    //private void Player_OnPickedUpSomething(object sender, System.EventArgs e)
    //{
    //    PlaySound(audioClipsRefsSO.objectPickup,(sender as Player).transform.position);
    //}

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
    {
        PlaySound(audioClipsRefsSO.chop,(sender as CuttingCounter).transform.position);
    }

    private void DeliveryManager_OnRecipeFailure(object sender, System.EventArgs e)
    {
        PlaySound(audioClipsRefsSO.deliveryFail, DeliveryManager.Instance.transform.position);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        PlaySound(audioClipsRefsSO.deliverySuccess, DeliveryManager.Instance.transform.position);
    }

    private void PlaySound(AudioClip[] clips,Vector3 position, float volume = 1f)
    {
        PlaySound(clips[Random.Range(0, clips.Length)], position, volume );
    }

    private void PlaySound(AudioClip clip, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(clip,position,volume);
    }

    public void PlayFootStepsSound(Vector3 position,float volumeMultiPlier)
    {
        PlaySound(audioClipsRefsSO.footsteps, position, volumeMultiPlier * volume);
    }

    public void PlayCountDownSound()
    {
        PlaySound(audioClipsRefsSO.warning, Vector3.zero);
    }

    public void PlayWarningSound(Vector3 position)
    {
        PlaySound(audioClipsRefsSO.warning[0], position);
    }

    public void ChangeSound()
    {
        volume += 0.1f;
        if(volume > 1f)
        {
            volume = 0f;
        }

        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUNDEFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }
}
