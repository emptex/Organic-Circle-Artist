using UnityEngine;
using UnityEngine.InputSystem;

public class DarkChasePlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float velocityThreshold = 0.1f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private float currentVelocity;
    private float verticalSpeed;
    private bool locked;

    public float CurrentVelocity => currentVelocity;
    public bool IsMoving => currentVelocity > velocityThreshold;

    public void Lock() { locked = true; currentVelocity = 0f; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (locked)
        {
            currentVelocity = 0f;
            return;
        }

        // New Input System: read W/S keys directly
        float input = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input += 1f;
            // S key disabled — player can only move forward
        }

        // Gravity
        if (controller.isGrounded)
            verticalSpeed = -0.5f;
        else
            verticalSpeed += gravity * Time.deltaTime;

        Vector3 move = transform.forward * input * moveSpeed;
        move.y = verticalSpeed;

        controller.Move(move * Time.deltaTime);
        currentVelocity = Mathf.Abs(input * moveSpeed);
    }
}
