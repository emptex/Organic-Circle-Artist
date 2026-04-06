using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DarkChaseDeathSequence : MonoBehaviour
{
    [SerializeField] private DarkChasePlayer player;
    [SerializeField] private DarkChaseAudioManager audioManager;
    [SerializeField] private AudioSource stingSource;
    [SerializeField] private CanvasGroup flashOverlay;
    [SerializeField] private CanvasGroup deathOverlay;
    [SerializeField] private GameObject restartButton;

    [Header("Timing")]
    [SerializeField] private float idleDeathTime = 7f;

    private float idleTimer;
    private bool isDead;

    private void Start()
    {
        if (restartButton != null)
            restartButton.SetActive(false);
    }

    private void Update()
    {
        if (isDead) return;

        if (player.IsMoving)
        {
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Time.deltaTime;
        }

        if (idleTimer >= idleDeathTime)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        isDead = true;

        // Lock player input
        player.Lock();

        // Stop audio manager tension updates
        if (audioManager != null)
            audioManager.enabled = false;

        // White flash
        flashOverlay.alpha = 1f;
        stingSource.Play();
        yield return new WaitForSeconds(0.15f);
        flashOverlay.alpha = 0f;

        // Fade in death overlay
        float t = 0f;
        float fadeDuration = 0.5f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            deathOverlay.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        deathOverlay.alpha = 1f;
        deathOverlay.blocksRaycasts = false; // let clicks through to restart button

        // Show restart button
        if (restartButton != null)
            restartButton.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
