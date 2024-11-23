using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour {
    // Reference to the transform
    Transform t;

    [Header("Player Rotation")]
    public float sensitivity = 1f;       // Mouse sensitivity
    public float rotationMin = -60f;     // Min vertical angle
    public float rotationMax = 60f;      // Max vertical angle

    private float rotationX;             // Horizontal rotation
    private float rotationY;             // Vertical rotation

    [Header("Player Movement")]
    public float speed = 5f;             // Movement speed
    public float verticalSpeed = 3f;     // Up/Down swimming speed
    public float maxFloatHeight = 10f;   // Maximum height
    public float minFloatHeight = 0f;    // Minimum height

    [Header("Boost Settings")]
    public float boostSpeed = 20f;               // Speed during the boost
    public float boostDuration = 0.5f;           // How long the boost lasts
    public float slowBoostFactor = 0.1f;         // How much slower each extra boost gets
    public float screenShakeIntensity = 0.2f;    // Intensity of screen shake
    public float screenShakeDuration = 0.5f;     // Duration of screen shake
    public int boostsBeforeSlowdown = 3;          // Number of boosts before slowing down
    public int boostsBeforeExhaustion = 5;        // Number of boosts before exhaustion
    public float exhaustionDuration = 2f;          // Duration of exhaustion state (where the speed is reduced)
    public float minSpeedAfterExhaustion = 1f;    // Minimum speed after boost exhaustion
    public float boostCooldown = 3f;              // Time before you can boost again after exhaustion
    public float boostResetTime = 5f; // Time in seconds after which the boost count resets if no boosts are used
    

    [Header("Camera Settings")]
    public Camera playerCamera;    // Reference to the camera

    private Vector3 cameraStartPosition; // To store the camera's initial position

    private float currentSpeed;          // Tracks current movement speed
    private bool isBoosting;             // Whether the player is boosting
    private float boostEndTime;          // When the boost ends
    private int boostCount;              // Tracks how many boosts the player has used
    private bool isExhausted;            // Tracks if the player is exhausted
    private float shakeEndTime;          // When the screen shake ends
    private float exhaustionEndTime;     // When the exhaustion period ends
    private float exhaustionStartSpeed;  // Store speed when exhaustion starts
    private float cooldownEndTime;       // When the cooldown ends after exhaustion
    private float lastBoostTime;

    private void Start() {
        t = transform; // Cache the transform
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        currentSpeed = speed; // Initialize current speed to normal speed

        // Cache the starting position of the camera
        if (playerCamera != null) {
            cameraStartPosition = playerCamera.transform.localPosition;
        }
    }

    private void Update() {
        LookAround(); // Handle camera movement

        // Allow the cursor to unlock for debugging
        if (Input.GetKey(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
        }

        if (!isExhausted && Time.time >= cooldownEndTime) {
            HandleBoost(); // Handle boost input only if cooldown has finished
        } else if (isExhausted) {
            HandleExhaustion(); // Handle exhaustion state
        }

        if (isExhausted) {
            HandleScreenShake(); // Apply screen shake when exhausted
        }
    }

    private void FixedUpdate() {
        if (!isExhausted) {
            Move(); // Handle player movement
        }
    }

    void LookAround() {
        // Get mouse input for camera rotation
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY += Input.GetAxis("Mouse Y") * sensitivity;

        // Clamp vertical rotation
        rotationY = Mathf.Clamp(rotationY, rotationMin, rotationMax);

        // Apply rotation to the player
        t.localRotation = Quaternion.Euler(-rotationY, rotationX, 0);
    }

    void Move() {
        // Get movement input
        float moveX = Input.GetAxis("Horizontal"); // Left/Right
        float moveZ = Input.GetAxis("Vertical");   // Forward/Backward
        float moveY = 0f;                          // Up/Down (will set below)

        // Handle vertical movement (Space/Shift)
        if (Input.GetKey(KeyCode.Space)) {
            moveY = 1f; // Move up
        } else if (Input.GetKey(KeyCode.LeftShift)) {
            moveY = -1f; // Move down
        }

        // Create movement vector relative to the camera's facing direction
        Vector3 movement = t.TransformDirection(new Vector3(moveX, 0, moveZ)) * currentSpeed * Time.deltaTime;

        // Add vertical movement
        movement += new Vector3(0, moveY * verticalSpeed * Time.deltaTime, 0);

        // Update position
        Vector3 newPosition = t.position + movement;

        // Clamp height between min and max float height
        newPosition.y = Mathf.Clamp(newPosition.y, minFloatHeight, maxFloatHeight);

        // Apply the new position
        t.position = newPosition;
    }

    void HandleBoost() {
        // Reset the boost count if enough time has passed since the last boost
        if (Time.time - lastBoostTime >= boostResetTime) {
            boostCount = 0;
            Debug.Log("Boost count reset due to inactivity.");
        }

        // Start the boost when pressing Q
        if (Input.GetKeyDown(KeyCode.Q) && !isBoosting) {
            isBoosting = true;
            boostCount++;
            lastBoostTime = Time.time; // Update the time of the last boost

            // Handle speed adjustment based on boost count
            if (boostCount > boostsBeforeSlowdown) {
                // Slow down the boost speed after the specified number of boosts
                float speedMultiplier = Mathf.Max(1f - (boostCount - boostsBeforeSlowdown) * slowBoostFactor, 0.5f);
                currentSpeed = boostSpeed * speedMultiplier;
            } else {
                currentSpeed = boostSpeed; // Normal boost speed before the slowdown threshold
            }

            // Exhaust the player after the specified number of boosts
            if (boostCount > boostsBeforeExhaustion) {
                isExhausted = true;
                exhaustionStartSpeed = currentSpeed; // Save speed at exhaustion
                exhaustionEndTime = Time.time + exhaustionDuration;
                currentSpeed = Mathf.Max(exhaustionStartSpeed * 0.1f, minSpeedAfterExhaustion); // Slow down during exhaustion
                shakeEndTime = Time.time + screenShakeDuration;

                Debug.Log("Player fully exhausted.");
            } else {
                Debug.Log($"Boost count: {boostCount}");
            }

            boostEndTime = Time.time + boostDuration;
        }

        // End the boost after the duration
        if (isBoosting && Time.time >= boostEndTime) {
            isBoosting = false;
            if (!isExhausted) {
                currentSpeed = speed; // Reset to normal speed
            }
        }
    }

    void HandleExhaustion() {
        if (Time.time >= exhaustionEndTime) {
            // Restore normal speed after exhaustion
            isExhausted = false;
            currentSpeed = speed; // Reset to normal speed
            cooldownEndTime = Time.time + boostCooldown; // Set cooldown time before the next boost
            boostCount = 0;

            // Log when the player is fully exhausted and can boost again
            Debug.Log("Exhaustion period over. Cooldown starts. Boost cooldown ends at: " + cooldownEndTime);
        }
    }

    void HandleScreenShake() {
        if (playerCamera == null) {
            Debug.LogError("Player Camera is not assigned!");
            return;
        }

        if (Time.time <= shakeEndTime) {
            // Randomize camera position slightly to simulate screen shake
            Vector3 shakeOffset = new Vector3(
                Random.Range(-screenShakeIntensity, screenShakeIntensity),
                Random.Range(-screenShakeIntensity, screenShakeIntensity),
                Random.Range(-screenShakeIntensity, screenShakeIntensity)
            );

            // Apply the shake offset to the camera's local position
            playerCamera.transform.localPosition = cameraStartPosition + shakeOffset;
        } else {
            // Reset the camera position to its starting position
            playerCamera.transform.localPosition = cameraStartPosition;
        }
    }

}