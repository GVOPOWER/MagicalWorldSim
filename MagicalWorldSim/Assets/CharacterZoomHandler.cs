using UnityEngine;

public class CharacterZoomHandler : MonoBehaviour
{
    public float zoomThreshold = 20f;     // Zoom level threshold
    public Sprite zoomedOutSprite;        // The sprite to use when zoomed out

    private Camera mainCamera;            // Reference to the main camera
    private Sprite originalSprite;        // The original sprite of the character
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool isZoomedOut = false;     // Track the current zoom state

    void Start()
    {
        // Find the main camera in the scene
        mainCamera = Camera.main;

        // Ensure main camera is found
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Please ensure the camera is tagged as 'MainCamera'.");
            return;
        }

        // Get the SpriteRenderer and Animator components
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Save the original sprite
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
    }

    void Update()
    {
        // Return if the main camera is not set
        if (mainCamera == null)
        {
            return;
        }

        // Check the camera's zoom level
        bool shouldZoomOut = mainCamera.orthographicSize > zoomThreshold;

        // Update sprite and animation state based on zoom level
        if (shouldZoomOut != isZoomedOut)
        {
            isZoomedOut = shouldZoomOut;
            UpdateCharacterAppearance();
        }
    }

    private void UpdateCharacterAppearance()
    {
        if (isZoomedOut)
        {
            // Change to the zoomed out sprite and disable animations
            spriteRenderer.sprite = zoomedOutSprite;
            if (animator != null)
            {
                animator.enabled = false;
            }
        }
        else
        {
            // Revert to the original sprite and enable animations
            spriteRenderer.sprite = originalSprite;
            if (animator != null)
            {
                animator.enabled = true;
            }
        }
    }
}
