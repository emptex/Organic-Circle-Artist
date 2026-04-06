using UnityEngine;

public class DarkChaseCorridorLoop : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform startAnchor;
    [SerializeField] private float corridorLength = 20f;

    private CharacterController playerController;

    private void Awake()
    {
        playerController = player.GetComponent<CharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform != player) return;

        // Only shift Z back by corridorLength, keep X and Y unchanged
        playerController.enabled = false;
        Vector3 pos = player.position;
        pos.z -= corridorLength;
        player.position = pos;
        playerController.enabled = true;
    }
}
