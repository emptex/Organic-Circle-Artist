using UnityEngine;

public class DarkChaseLightFlicker : MonoBehaviour
{
    [SerializeField] private Light targetLight;
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 20f;
    [SerializeField] private float flickerSpeed = 0.1f;

    private float timer;

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (Random.value < 0.15f)
        {
            // Occasionally go fully dark for a beat
            targetLight.intensity = 0f;
            timer = Random.Range(0.3f, 1.0f);
        }
        else
        {
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);
            timer = Random.Range(flickerSpeed * 0.5f, flickerSpeed * 2f);
        }
    }
}
