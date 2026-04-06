using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DarkChasePostProcessing : MonoBehaviour
{
    [SerializeField] private DarkChaseAudioManager audioManager;
    [SerializeField] private Volume globalVolume;

    [Header("Vignette")]
    [SerializeField] private float vignetteMin = 0.3f;
    [SerializeField] private float vignetteMax = 0.6f;

    [Header("Chromatic Aberration")]
    [SerializeField] private float chromaticMin = 0f;
    [SerializeField] private float chromaticMax = 0.8f;

    [Header("Film Grain")]
    [SerializeField] private float grainMin = 0.1f;
    [SerializeField] private float grainMax = 0.7f;

    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;

    private void Start()
    {
        if (globalVolume == null || globalVolume.profile == null)
        {
            enabled = false;
            return;
        }

        // If overrides are missing from the profile, add them at runtime
        if (!globalVolume.profile.TryGet(out vignette))
        {
            vignette = globalVolume.profile.Add<Vignette>();
            vignette.intensity.Override(vignetteMin);
        }
        if (!globalVolume.profile.TryGet(out chromaticAberration))
        {
            chromaticAberration = globalVolume.profile.Add<ChromaticAberration>();
            chromaticAberration.intensity.Override(chromaticMin);
        }
        if (!globalVolume.profile.TryGet(out filmGrain))
        {
            filmGrain = globalVolume.profile.Add<FilmGrain>();
            filmGrain.intensity.Override(grainMin);
        }
    }

    private void Update()
    {
        float t = audioManager.ChaseVolume;
        vignette.intensity.value = Mathf.Lerp(vignetteMin, vignetteMax, t);
        chromaticAberration.intensity.value = Mathf.Lerp(chromaticMin, chromaticMax, t);
        filmGrain.intensity.value = Mathf.Lerp(grainMin, grainMax, t);
    }
}
