using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private Canvas canvas;
    private GameObject uiPanel;
    private RandomWalker randomWalker;
    private CityCreation cityCreation;

    private Text characterInfoText;
    private Text cityInfoText;

    private void Start()
    {
        // Find existing components
        randomWalker = FindObjectOfType<RandomWalker>();
        cityCreation = FindObjectOfType<CityCreation>();

        // Create a Canvas if it doesn't exist
        if (FindObjectOfType<Canvas>() == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvas = FindObjectOfType<Canvas>();
        }

        // Create a UI panel
        uiPanel = new GameObject("UIPanel");
        uiPanel.transform.SetParent(canvas.transform);
        RectTransform panelRect = uiPanel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(300, 150);
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);

        // Create Text for character info
        characterInfoText = CreateUITextElement("CharacterInfo", uiPanel.transform, new Vector2(10, -30), 14);

        // Create Text for city info
        cityInfoText = CreateUITextElement("CityInfo", uiPanel.transform, new Vector2(10, -70), 14);
    }

    private Text CreateUITextElement(string name, Transform parent, Vector2 position, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = Color.black;
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(280, 30);
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(0, 1);
        textRect.pivot = new Vector2(0, 1);
        textRect.anchoredPosition = position;
        return text;
    }

    private void Update()
    {
        if (randomWalker != null)
        {
            characterInfoText.text = $"Name: {randomWalker.attributes.characterName}\nAge: {Mathf.Floor(randomWalker.attributes.currentAge)}\nHunger: {Mathf.Floor(randomWalker.attributes.currentHunger)}";
        }

        if (cityCreation != null && !string.IsNullOrEmpty(randomWalker.attributes.characterName))
        {
            cityInfoText.text = $"Current City: {randomWalker.attributes.characterName}";
        }
        else
        {
            cityInfoText.text = "No City";
        }
    }
}
