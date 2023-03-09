using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystem : MonoBehaviour
{
    [System.NonSerialized]
    public SolarSystemData data;

    [Delayed]
    public int seed;

    [Range(2, 256)]
    public int celestialBodyResolution = 10;

    public Material planetMaterial;

    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;

    private void Awake()
    {
        //Randomize();
    }

    void Randomize()
    {
        Random.InitState(seed);
        data = new SolarSystemData();
        data.sunBody = new CelestialBodyData();
        data.sunBody.type = CelestialBodyType.Star;
        float temperature = Random.Range(17f, 50f);
        data.sunBody.color = CelestialUtilities.TemperatureToColor(temperature * 100f);
        data.sunBody.radius = Random.Range(5f, 10f);
        data.sunBody.lightIntensity = 300 * (100 / data.sunBody.radius);
        data.sunBody.rotationAxis = Vector3.up;

        int planets = Random.Range(1, 10);
        float distance = data.sunBody.radius;
        data.sunBody.orbitingBodies = new CelestialBodyData[planets];

        for (int i = 0; i < planets; i++)
        {
            var body = new CelestialBodyData();
            body.type = CelestialBodyType.Planet;
            distance += Random.Range(10f, 30f);
            body.orbitDistance = distance;
            body.radius = Random.Range(0.75f, 2.5f);
            body.orbitingBodies = new CelestialBodyData[0];
            body.rotationSpeed = Random.Range(20f, 50f);
            body.rotationAxis = Vector3.up;
            if (Random.Range(0, 1f) < 0.2f) body.rotationAxis = Vector3.left;
            body.orbitAxis = Vector3.up;
            body.orbitSpeed = 20 * (100 / distance);
            body.initialOrbitProgress = Random.Range(0f, 1f);
            data.sunBody.orbitingBodies[i] = body;
        }

        data.sunBody.lightRange = data.sunBody.orbitingBodies[planets - 1].orbitDistance * 1.25f;

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
        celestialBody.material = planetMaterial;
        celestialBody.data = body;
        celestialBody.resolution = celestialBodyResolution;
        var shapeSettings = new ShapeSettings();
        shapeSettings.planetRadius = body.radius;
        shapeSettings.noiseLayers = new ShapeSettings.NoiseLayer[0];
        if(body.type == CelestialBodyType.Planet)
        shapeSettings.noiseLayers = this.shapeSettings.noiseLayers;
        celestialBody.shapeSettings = shapeSettings;
        celestialBody.colorSettings = colorSettings;
        go.transform.parent = parent;
        go.transform.localPosition = new Vector3(0, 0, distance);
        go.transform.RotateAround(go.transform.parent.position, body.orbitAxis, 360f * body.initialOrbitProgress);

        foreach(var child in body.orbitingBodies)
        {
            CreateBody(distance + child.orbitDistance, go.transform, child);
        }
    }

    public void Recreate()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Randomize();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        Recreate();
    }
}

[System.Serializable]
public class SolarSystemData
{
    public CelestialBodyData sunBody;
}
