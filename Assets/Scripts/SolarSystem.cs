using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystem : MonoBehaviour
{
    public SolarSystemData data;

    public Color[] possibleStarColors;
    public Color[] possiblePlanetColors;

    [Delayed]
    public int seed;

    [Range(2, 256)]
    public int celestialBodyResolution = 10;

    private void Awake()
    {
        Randomize();
    }

    void Randomize()
    {
        Random.InitState(seed);
        data = new SolarSystemData();
        data.sunBody = new CelestialBodyData();
        data.sunBody.type = CelestialBodyType.Star;
        data.sunBody.color = possibleStarColors[Random.Range(0, possiblePlanetColors.Length)];
        data.sunBody.radius = Random.Range(5f, 10f);

        int planets = Random.Range(1, 5);
        float distance = 0;
        data.sunBody.orbitingBodies = new CelestialBodyData[planets];

        for (int i = 0; i < planets; i++)
        {
            var body = new CelestialBodyData();
            body.type = CelestialBodyType.Planet;
            distance += Random.Range(10f, 30f);
            body.orbitDistance = distance;
            body.radius = Random.Range(0.5f, 2f);
            body.color = possiblePlanetColors[Random.Range(0, possiblePlanetColors.Length)];
            body.orbitingBodies = new CelestialBodyData[0];
            data.sunBody.orbitingBodies[i] = body;
        }

        CreateSystem();
    }

    void CreateSystem()
    {
        CreateBody(0, transform, data.sunBody);
    }

    void CreateBody(float distance, Transform parent, CelestialBodyData body)
    {
        GameObject go = new GameObject();
        var celestialBody = go.AddComponent<CelestialBody>();
        celestialBody.data = body;
        celestialBody.resolution = celestialBodyResolution;
        go.transform.parent = parent;
        go.transform.localPosition = new Vector3(0, 0, distance);

        foreach(var child in body.orbitingBodies)
        {
            CreateBody(distance + child.orbitDistance, go.transform, child);
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Randomize();
    }
}

[System.Serializable]
public class SolarSystemData
{
    public CelestialBodyData sunBody;
}
