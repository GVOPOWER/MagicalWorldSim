using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance; // Singleton instance

    [SerializeField] private float panSpeed = 0.5f; // Speed of camera following the target
    [SerializeField] private float lateralSpeed = 2f; // Speed of camera when dragging manually
    [SerializeField] private float minZoom = 5f; // Minimum zoom level
    [SerializeField] private float maxZoom = 20f; // Maximum zoom level
    [SerializeField] private Camera cam; // Reference to the Camera
    [SerializeField] private float ZoomSpeed = 20f; // Maximum zoom level

    private Transform target; // The current target for the camera

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        HandleCameraMovement();
        HandleCameraZoom();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget; // Set the target to follow
    }

    private void HandleCameraMovement()
    {
        if (target != null)
        {
            // Follow the target
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, cam.transform.position.z);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, panSpeed);
        }
        else
        {
            // Manual camera movement when no target is set
            if (Input.GetMouseButton(1)) // Right mouse button for dragging
            {
                float moveX = Input.GetAxis("Mouse X") * lateralSpeed;
                float moveY = Input.GetAxis("Mouse Y") * lateralSpeed;

                cam.transform.position -= new Vector3(moveX, moveY, 0);
            }
        }
    }

    private void HandleCameraZoom()
    {
        // Zoom with mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * ZoomSpeed, minZoom, maxZoom);
        }
    }
}
