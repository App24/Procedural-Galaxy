using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SolarSystem : MonoBehaviour
{
    [System.NonSerialized]
    public SolarSystemData data;

    [Delayed]
    public int seed;

    [Range(2, 256), Delayed]
    public int celestialBodyResolution = 10;

    [Delayed]
    public float lightIntensity = 200;

    [Delayed]
    public float lightRadius = 1.25f;

    [Delayed]
    public float orbitSpeed = 20f;

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
        data.sunBody.radius = Random.Range(10f, 20f);
        data.sunBody.lightIntensity = lightIntensity * (100 / data.sunBody.radius);
        data.sunBody.rotationAxis = Vector3.up;

        int planets = Random.Range(1, 10);
        float distance = data.sunBody.radius;
        data.sunBody.orbitingBodies = new CelestialBodyData[planets];

        for (int i = 0; i < planets; i++)
        {
            var body = new CelestialBodyData();
            body.type = CelestialBodyType.Planet;
            distance += Random.Range(20f, 40f);
            body.orbitDistance = distance;
            body.radius = Random.Range(0.75f, 3f);
            body.orbitingBodies = new CelestialBodyData[0];
            body.rotationSpeed = Random.Range(20f, 50f);
            body.rotationAxis = Vector3.up;
            if (Random.value < 0.2f) body.rotationAxis = Vector3.left;
            body.orbitAxis = Vector3.up;
            body.orbitSpeed = orbitSpeed * (100 / distance);
            body.tidalLocked = Random.value < 0.2f;
            body.initialOrbitProgress = Random.value;
            data.sunBody.orbitingBodies[i] = body;
        }

        data.sunBody.lightRange = data.sunBody.orbitingBodies[planets - 1].orbitDistance * lightRadius;

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
        celestialBody.resolution = body.type == CelestialBodyType.Planet ? celestialBodyResolution : 20;
        var shapeSettings = new ShapeSettings();
        shapeSettings.planetRadius = body.radius;
        shapeSettings.noiseLayers = new ShapeSettings.NoiseLayer[0];
        if (body.type == CelestialBodyType.Planet)
        {
            shapeSettings.noiseLayers = new ShapeSettings.NoiseLayer[this.shapeSettings.noiseLayers.Length];
            for (int i = 0; i < shapeSettings.noiseLayers.Length; i++)
            {
                shapeSettings.noiseLayers[i] = this.shapeSettings.noiseLayers[i].CloneViaFakeSerialization();
                {
                    shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.center = Random.insideUnitSphere * Random.Range(0, 5000f);
                    shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.baseRoughness += Random.Range(-0.5f, 0.5f);
                    shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.minValue += Random.Range(-0.5f, 0.5f);
                    shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.persistence += Random.Range(-0.05f, 0.05f);
                    shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.roughness += Random.Range(-0.5f, 0.5f);
                }
                {
                    shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.center = Random.insideUnitSphere * Random.Range(0, 5000f);
                    shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.baseRoughness += Random.Range(-0.5f, 0.5f);
                    shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.minValue += Random.Range(-0.5f, 0.5f);
                    shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.persistence += Random.Range(-0.05f, 0.05f);
                    shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.roughness += Random.Range(-0.5f, 0.5f);
                }
            }
        }
        celestialBody.shapeSettings = shapeSettings;
        celestialBody.colorSettings = colorSettings;
        go.transform.parent = parent;
        go.transform.localPosition = new Vector3(0, 0, distance);
        go.transform.RotateAround(go.transform.parent.position, body.orbitAxis, 360f * body.initialOrbitProgress);

        foreach (var child in body.orbitingBodies)
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
