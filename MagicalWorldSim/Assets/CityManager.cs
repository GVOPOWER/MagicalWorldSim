using UnityEngine;
using System.Collections.Generic;

public class CityManager : MonoBehaviour
{
    public List<City> cities = new List<City>();

    public void CreateCity(string name, Vector3Int position, int maxPopulation, int maxHouses)
    {
        var newCity = new City(name, position, maxPopulation, maxHouses);
        cities.Add(newCity);
        Debug.Log($"City created: {newCity.cityName}, Position: {newCity.position}, Max Population: {newCity.maxPopulation}, Max Houses: {newCity.maxHouses}");

        LogAllCities();
    }

    private void LogAllCities()
    {
        Debug.Log("Current Cities in Manager:");
        foreach (var city in cities)
        {
            Debug.Log($"City: {city.cityName}, Position: {city.position}, Population: {city.currentPopulation}");
        }
    }
}
