using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private Player player;
    private float footStepsTimer;
    private float footStepsTimerMax = 0.1f;

    private void Awake()
    {
        player = GetComponent<Player>();
    }
    private void Update()
    {
        footStepsTimer -= Time.deltaTime;
        if(footStepsTimer < 0)
        {
            footStepsTimer = footStepsTimerMax;

            if (player.isMoving.Value)
            {
                float volume = 1f;
                SoundManager.Instance.PlayFootStepsSound(player.transform.position, volume);
            }
        }
    }
}
