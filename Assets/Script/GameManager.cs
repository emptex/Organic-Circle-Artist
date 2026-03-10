using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera mainCamera;
    [SerializeField] PlayerController player;
    [SerializeField] TextMeshProUGUI drawText;
    [SerializeField] GameObject replayButton;
    [SerializeField] Volume globalVolume;

    [Header("Session Settings")]
    [SerializeField] float drawDuration = 7f;
    [SerializeField] float zoomDuration = 3f;
    [SerializeField] float startCamSize = 5f;
    [SerializeField] float endCamSize = 20f;

    [Header("Bloom")]
    [SerializeField] float bloomIntensity = 1f;
    [SerializeField] float bloomScatter = 0.7f;

    float startTime;
    bool zoomStarted;
    bool sessionEnded;

    void Start()
    {
        TuneBloom();
        replayButton.SetActive(false);
        startTime = Time.time;
    }

    void TuneBloom()
    {
        if (globalVolume != null && globalVolume.profile.TryGet<Bloom>(out var bloom))
        {
            bloom.intensity.value = bloomIntensity;
            bloom.scatter.value = bloomScatter;
        }
    }

    void Update()
    {
        float elapsed = Time.time - startTime;

        // Fade "Draw freely" text over 2 seconds
        if (drawText != null && elapsed < 2f)
        {
            float alpha = 1f - elapsed / 2f;
            drawText.color = new Color(drawText.color.r, drawText.color.g, drawText.color.b, alpha);
        }
        else if (drawText != null && elapsed >= 2f)
        {
            drawText.gameObject.SetActive(false);
        }

        // Zoom-out phase
        if (elapsed > drawDuration && !sessionEnded)
        {
            if (!zoomStarted)
            {
                zoomStarted = true;
                player.canMove = false;
            }

            float zoomT = Mathf.Clamp01((elapsed - drawDuration) / zoomDuration);
            float t = 1f - Mathf.Pow(1f - zoomT, 3f); // ease out cubic
            mainCamera.orthographicSize = Mathf.Lerp(startCamSize, endCamSize, t);

            if (zoomT >= 1f)
            {
                sessionEnded = true;
                replayButton.SetActive(true);
            }
        }
    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
