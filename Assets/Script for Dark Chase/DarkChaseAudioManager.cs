using UnityEngine;

public class DarkChaseAudioManager : MonoBehaviour
{
    [SerializeField] private DarkChasePlayer player;
    [SerializeField] private AudioSource chaseDroneSource;
    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private AudioSource footstepSource;

    [Header("Volume Curve")]
    [SerializeField] private float decayRate = 0.15f;
    [SerializeField] private float rampRate = 0.25f;

    [Header("Heartbeat")]
    [SerializeField] private float heartbeatMinVolume = 0.15f;
    [SerializeField] private float heartbeatMaxVolume = 1f;

    [Header("Footsteps")]
    [SerializeField] private float footstepInterval = 0.45f;

    private float chaseVolume = 0.3f;
    private float footstepTimer;

    public float ChaseVolume => chaseVolume;

    private void Update()
    {
        if (player.IsMoving)
        {
            chaseVolume -= decayRate * Time.deltaTime;
            HandleFootsteps();
        }
        else
        {
            chaseVolume += rampRate * Time.deltaTime;
            footstepTimer = 0f;
        }

        chaseVolume = Mathf.Clamp01(chaseVolume);
        chaseDroneSource.volume = chaseVolume;

        // Heartbeat always audible, gets louder when stopped
        heartbeatSource.volume = Mathf.Lerp(heartbeatMinVolume, heartbeatMaxVolume, chaseVolume);
    }

    private void HandleFootsteps()
    {
        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepInterval)
        {
            footstepSource.Play();
            footstepTimer = 0f;
        }
    }
}
