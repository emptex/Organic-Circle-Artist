using UnityEngine;

/// <summary>
/// Generates placeholder audio clips at runtime so the game works without importing audio files.
/// Attach to the AudioManager GameObject. On Awake it fills any empty AudioSource clips.
/// </summary>
public class DarkChaseAudioGenerator : MonoBehaviour
{
    [SerializeField] private AudioSource chaseDroneSource;
    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource stingSource;

    private void Awake()
    {
        if (chaseDroneSource != null && chaseDroneSource.clip == null)
            chaseDroneSource.clip = GenerateDrone(4f, 80);

        if (heartbeatSource != null && heartbeatSource.clip == null)
            heartbeatSource.clip = GenerateHeartbeat(2f);

        if (footstepSource != null && footstepSource.clip == null)
            footstepSource.clip = GenerateFootstep();

        if (stingSource != null && stingSource.clip == null)
            stingSource.clip = GenerateSting();
    }

    static AudioClip GenerateDrone(float duration, float freq)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            // Low rumbling drone with slight modulation
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f
                       + Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * 0.15f
                       + Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t) * 0.2f;
            // Slow amplitude modulation for unease
            samples[i] *= 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 0.3f * t);
        }
        var clip = AudioClip.Create("Drone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    static AudioClip GenerateHeartbeat(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        float bps = 80f / 60f; // 80 BPM
        float beatInterval = 1f / bps;
        float doubleHitGap = 0.12f; // "lub-dub" gap

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float inBeat = t % beatInterval;

            float pulse = 0f;
            // First thump (lub)
            pulse += BeatPulse(inBeat, 0f, 60f);
            // Second thump (dub) slightly higher pitch
            pulse += BeatPulse(inBeat, doubleHitGap, 75f) * 0.7f;

            samples[i] = pulse;
        }
        var clip = AudioClip.Create("Heartbeat", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    static float BeatPulse(float inBeat, float offset, float freq)
    {
        float local = inBeat - offset;
        if (local < 0f || local > 0.1f) return 0f;
        float envelope = Mathf.Exp(-local * 40f); // fast decay
        return Mathf.Sin(2f * Mathf.PI * freq * local) * envelope * 0.8f;
    }

    static AudioClip GenerateFootstep()
    {
        int sampleRate = 44100;
        float duration = 0.15f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 30f);
            // Noise burst for concrete footstep
            samples[i] = (Random.value * 2f - 1f) * envelope * 0.5f;
        }
        var clip = AudioClip.Create("Footstep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    static AudioClip GenerateSting()
    {
        int sampleRate = 44100;
        float duration = 1.5f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 2f);
            // Harsh dissonant chord
            samples[i] = (Mathf.Sin(2f * Mathf.PI * 180f * t)
                        + Mathf.Sin(2f * Mathf.PI * 227f * t)
                        + Mathf.Sin(2f * Mathf.PI * 340f * t)
                        + (Random.value * 2f - 1f) * 0.3f)
                        * envelope * 0.6f;
        }
        var clip = AudioClip.Create("Sting", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
