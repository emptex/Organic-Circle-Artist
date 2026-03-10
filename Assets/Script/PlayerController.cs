using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float baseAmplitude = 0.3f;
    [SerializeField] float rampRate = 0.5f;

    [HideInInspector] public bool canMove = true;

    float noiseSeedX;
    float noiseSeedY;
    float noiseTime;
    TrailRenderer trail;

    void Start()
    {
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(0f, 1000f);

        trail = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (!canMove) return;

        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        noiseTime += Time.deltaTime;
        float amplitude = baseAmplitude + noiseTime * rampRate;
        float noiseX = (Mathf.PerlinNoise(noiseSeedX + noiseTime * 0.5f, 0f) - 0.5f) * 2f * amplitude;
        float noiseY = (Mathf.PerlinNoise(0f, noiseSeedY + noiseTime * 0.5f) - 0.5f) * 2f * amplitude;

        Vector3 move = new Vector3(inputX + noiseX, 0f, inputY + noiseY);
        transform.position += move * moveSpeed * Time.deltaTime;

        float speed = move.magnitude * moveSpeed;
        Color c = GetColorFromSpeed(speed);
        trail.startColor = c;
        trail.endColor = c;
    }

    Color GetColorFromSpeed(float speed)
    {
        float t = Mathf.Clamp01(speed / (moveSpeed * 1.2f));
        float hue = Mathf.Lerp(0.05f, 0.65f, t);
        return Color.HSVToRGB(hue, 0.9f, 2.5f, true);
    }
}
