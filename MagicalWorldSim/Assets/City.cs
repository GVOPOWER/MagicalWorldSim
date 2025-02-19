using UnityEngine;

public class City : MonoBehaviour
{
    public int population;
    public float growthRate = 1.1f; // Rate at which the city might expand its influence
    public float expansionRate = 1.05f; // Rate at which the city's area grows
    public float updateInterval = 10f; // Interval to update city growth
    private float lastUpdateTime = -Mathf.Infinity;

    private void Start()
    {
        // Initialize population
        population = 0;
    }

    private void Update()
    {
        if (Time.time >= lastUpdateTime + updateInterval)
        {
            ExpandCityArea();
            lastUpdateTime = Time.time;
        }
    }

    private void ExpandCityArea()
    {
        // Optionally expand the city's area over time
        transform.localScale *= expansionRate;
        Debug.Log($"City expanded! Scale: {transform.localScale}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            population++;
            Debug.Log($"Player entered city. Current population: {population}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            population--;
            Debug.Log($"Player left city. Current population: {population}");
        }
    }
}
