using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform currentTarget;  // The current target to follow
    public float smoothSpeed = 0.125f; // Speed of camera follow
    public Vector2 offset; // Offset from the target
    public float heightOffset = 2f; // Height offset to keep above the character

    private Vector3 originalCameraPosition; // To store the original camera position

    private void Start()
    {
        // Store the original camera position
        originalCameraPosition = transform.position;
    }

    private void Update()
    {
        // Check for mouse clicks to change the target
        if (Input.GetMouseButtonDown(0))
        {
            CheckForTargetChange();
        }
    }

    private void LateUpdate()
    {
        if (currentTarget != null)
        {
            // Calculate the new camera position with offset
            Vector3 desiredPosition = new Vector3(currentTarget.position.x + offset.x, currentTarget.position.y + heightOffset, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        }
        else
        {
            // Optionally, you can keep the camera at the original position when no target is set
            // transform.position = originalCameraPosition;
        }
    }

    private void CheckForTargetChange()
    {
        // Raycast from the mouse position to detect if a character is clicked
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure we're working in 2D space

        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);
        if (hitCollider != null && hitCollider.CompareTag("Player")) // Check if the clicked object has the "Character" tag
        {
            // Set the new target if a character is clicked
            currentTarget = hitCollider.transform;
        }
        else
        {
            // Set the target to null if clicking on something that is not a character
            currentTarget = null;
        }
    }
}
