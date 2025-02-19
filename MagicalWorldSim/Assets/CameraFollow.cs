using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The target to follow
    public float smoothSpeed = 0.125f; // Speed of camera follow
    public Vector2 offset; // Offset from the target
    public float heightOffset = 2f; // Height offset to keep above the character

    private Vector3 originalCameraPosition; // To store the original camera position
    private bool isFollowing = false; // To track if the camera is currently following the target

    private void Start()
    {
        // Store the original camera position
        originalCameraPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (isFollowing && target != null)
        {
            // Create a desired position based on the target's position and the offset
            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y + heightOffset, -10);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Optionally, make the camera look at the target
            // Note: In 2D, this is usually not necessary since we want a fixed orthographic view
        }
    }

    public void ToggleFollow()
    {
        isFollowing = !isFollowing; // Toggle following state

        if (!isFollowing)
        {
            // Stop following and return camera to original position
            transform.position = originalCameraPosition;
        }
    }
}
