using UnityEngine;
using UnityEngine.UI; // Keep this if you are still using UI elements
using TMPro; // Add this for TextMeshPro

public class GameSpeedController : MonoBehaviour
{
    public float speed = 1;
    public Slider speedSlider; // Reference to the UI Slider component
    public TMP_Text speedText; // Reference to the TextMeshPro UI component to display the current speed

    private void Start()
    {
        if (speedSlider != null)
        {
            // Set the default slider value and add a listener for value changes
            speedSlider.value = speed; // Normal game speed
            speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
        }

        if (speedText != null)
        {
            UpdateSpeedText(speedSlider.value);
        }
    }

    private void OnSpeedSliderChanged(float value)
    {
        // Change the game's time scale based on the slider's value
        Time.timeScale = value;

        // Update the speed text display
        UpdateSpeedText(value);
    }

    private void UpdateSpeedText(float value)
    {
        if (speedText != null)
        {
            speedText.text = $"Game Speed: {value:0.0}x"; // Format the text
        }
    }
}
