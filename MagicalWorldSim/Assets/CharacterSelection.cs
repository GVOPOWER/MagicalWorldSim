using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    private CameraFollow cameraFollowScript;
    private bool isFollowing = false; // Track whether we are following this character

    private void Start()
    {
        cameraFollowScript = Camera.main.GetComponent<CameraFollow>();
    }

    private void OnMouseDown()
    {
        // Toggle follow state
        isFollowing = !isFollowing;

        if (isFollowing)
        {
            Debug.Log("Following Character: " + gameObject.name);
            cameraFollowScript.target = transform; // Set the target to this character
            cameraFollowScript.ToggleFollow(); // Start following this character
        }
        else
        {
            Debug.Log("Stopped Following Character: " + gameObject.name);
            cameraFollowScript.ToggleFollow(); // Stop following this character
            cameraFollowScript.target = null; // Optionally clear the target if needed
        }
    }
}
