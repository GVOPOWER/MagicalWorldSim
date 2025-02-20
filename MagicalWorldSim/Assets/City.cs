using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // For Text

public class City : MonoBehaviour
{
    public Text cityNameText;
    public List<RandomWalker> citizens = new List<RandomWalker>();
    private string cityName;
    private SpriteRenderer overlaySpriteRenderer;
    private float cityGrowthRate = 0.1f; // Adjust the growth rate as needed

    private void Start()
    {
        overlaySpriteRenderer = GetComponent<SpriteRenderer>();
        overlaySpriteRenderer.color = new Color(1, 1, 1, 0.5f);
        SetCityName();
    }

    private void Update()
    {
        GrowCity();
    }

    public void SetCityName()
    {
        CityManager cityManager = FindObjectOfType<CityManager>();
        if (cityManager != null)
        {
            string namePart1 = cityManager.cityNamesPart1[Random.Range(0, cityManager.cityNamesPart1.Count)];
            string namePart2 = cityManager.cityNamesPart2[Random.Range(0, cityManager.cityNamesPart2.Count)];
            cityName = $"{namePart1} {namePart2}";
            cityNameText.text = cityName;
        }
    }

    public void AddCitizen(RandomWalker citizen)
    {
        if (!citizens.Contains(citizen))
        {
            citizens.Add(citizen);
        }
    }

    private void GrowCity()
    {
        transform.localScale += Vector3.one * cityGrowthRate * citizens.Count * Time.deltaTime;
    }
}
    