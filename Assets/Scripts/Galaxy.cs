using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Galaxy : MonoBehaviour
{
    public static int seed;

    [Range(2, 256), Delayed]
    public int celestialBodyResolution = 10;

    [Delayed]
    public float lightIntensity = 200;

    [Delayed]
    public float lightRadius = 1.25f;

    [Delayed]
    public float orbitSpeed = 20f;

    [Delayed, Min(1)]
    public int minPlanets = 1;

    [Delayed, Min(1)]
    public int maxPlanets = 10;

    [Delayed, Min(1)]
    public int minStars = 10;

    [Delayed, Min(1)]
    public int maxStars = 10;

    public Material sunMaterial;
    public Material planetMaterial;

    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;

    public Gradient dryOceanColor;
    public Gradient wetOceanColor;
    public Gradient frozenOceanColor;

    public Material trailMaterial;

    const string glyphs = "0123456789";

    private void Start()
    {
        Recreate();
    }

    void Randomize()
    {
        Random.InitState(seed);
        var stars = Random.Range(minStars, maxStars);
        float distance = 0;
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetParent(transform, false);
            go.GetComponent<MeshRenderer>().material = sunMaterial;
            go.transform.localScale = new Vector3(100, 100, 100);
        }
        for (int i = 0; i < stars; i++)
        {
            var solarSystemGo = new GameObject();
            solarSystemGo.transform.SetParent(transform, false);

            var solarSystem = solarSystemGo.AddComponent<SolarSystem>();
            solarSystem.seed = seed + i;
            solarSystem.minPlanets = minPlanets;
            solarSystem.maxPlanets = maxPlanets;
            solarSystem.lightIntensity = lightIntensity;
            solarSystem.lightRadius = lightRadius;
            solarSystem.planetMaterial = planetMaterial;
            solarSystem.orbitSpeed = orbitSpeed;
            solarSystem.celestialBodyResolution = celestialBodyResolution;
            solarSystem.shapeSettings = shapeSettings;
            solarSystem.colorSettings = colorSettings;
            solarSystem.dryOceanColor = dryOceanColor;
            solarSystem.wetOceanColor = wetOceanColor;
            solarSystem.frozenOceanColor = frozenOceanColor;
            solarSystem.trailMaterial = trailMaterial;
            solarSystem.sunMaterial = sunMaterial;

            solarSystem.Recreate();


            solarSystemGo.name = solarSystem.data.sunBody.name + " (Solar System)";

            if (solarSystem.data.sunBody.orbitingBodies.Length > 0)
                distance += (solarSystem.data.sunBody.orbitingBodies[solarSystem.data.sunBody.orbitingBodies.Length - 1].orbitDistance) * 5f;
            else distance += 20 * 5f;

            solarSystem.data.sunBody.orbitSpeed = orbitSpeed * (100f / distance);

            var newSolarSystem = solarSystemGo.transform.GetChild(0).transform;
            newSolarSystem.SetParent(transform, false);

            newSolarSystem.localPosition = new Vector3(0, 0, distance);
            newSolarSystem.RotateAround(transform.position, solarSystem.data.sunBody.orbitAxis, 360f * Random.value);

            Destroy(solarSystemGo);
        }
        /*data = new SolarSystemData();
        data.sunBody = new CelestialBodyData();
        string numbers = "";
        for (int i = 0; i < 6; i++)
        {
            numbers += glyphs[Random.Range(0, glyphs.Length)];
        }
        string starName = $"HIP {numbers}";
        data.sunBody.name = $"(Star) {starName}";
        data.sunBody.type = CelestialBodyType.Star;
        float temperature = Random.Range(17f, 50f);
        data.sunBody.temperature = temperature;
        data.sunBody.color = CelestialUtilities.TemperatureToColor(temperature * 100f);
        data.sunBody.radius = Random.Range(10f, 20f);
        data.sunBody.lightIntensity = lightIntensity * (100 / data.sunBody.radius);
        data.sunBody.rotationAxis = Vector3.up;

        int planets = Random.Range(1, maxPlanets);
        float distance = data.sunBody.radius;
        data.sunBody.orbitingBodies = new CelestialBodyData[planets];

        for (int i = 0; i < planets; i++)
        {
            var body = new CelestialBodyData();
            string planetName = $"{starName} {i + 1}";
            body.name = $"(Planet) {planetName}";
            body.type = CelestialBodyType.Planet;
            distance += Random.Range(20f, 40f);
            body.orbitDistance = distance;
            body.radius = Random.Range(0.75f, 6f);
            body.orbitingBodies = new CelestialBodyData[0];
            body.rotationSpeed = Random.Range(20f, 50f);
            body.rotationAxis = Vector3.up;
            if (Random.value < 0.2f) body.rotationAxis = Vector3.left;
            body.orbitAxis = Vector3.up;
            if (Random.value < 0.2f) body.orbitAxis = Vector3.down;
            body.orbitSpeed = orbitSpeed * (100 / distance);
            body.tidalLocked = Random.value < 0.2f;
            body.initialOrbitProgress = Random.value;
            if (Random.value < 0.1f)
            {
                var moon = new CelestialBodyData();
                moon.name = $"(Moon) {planetName}.1";
                moon.type = CelestialBodyType.Planet;
                moon.orbitDistance = Random.Range(body.radius * 1.5f, body.radius * 2.5f);
                moon.orbitingBodies = new CelestialBodyData[0];
                moon.rotationSpeed = Random.Range(20f, 50f);
                moon.rotationAxis = Vector3.up;
                moon.radius = Random.Range(body.radius / 20f, body.radius / 5f);
                moon.orbitSpeed = orbitSpeed * (100 / (moon.orbitDistance + body.radius));
                moon.orbitAxis = Vector3.up;
                if (Random.value < 0.2f) moon.orbitAxis = Vector3.down;
                moon.tidalLocked = Random.value < 0.2f;
                moon.initialOrbitProgress = Random.value;

                body.orbitingBodies = new CelestialBodyData[] { moon };
            }
            data.sunBody.orbitingBodies[i] = body;
        }

        var firstDistance = data.sunBody.orbitingBodies[0].orbitDistance;

        for (int i = 0; i < planets; i++)
        {
            var body = data.sunBody.orbitingBodies[i];
            if (planets <= 1)
                body.distanceFromStar = 0;
            else body.distanceFromStar = (body.orbitDistance - firstDistance) / (distance - firstDistance);
            body.waterAlbedo = body.distanceFromStar * 0.8f;
            body.albedo = Mathf.Clamp01(body.distanceFromStar - 0.5f) * 2 * 0.5f;
        }

        data.sunBody.lightRange = data.sunBody.orbitingBodies[planets - 1].orbitDistance * lightRadius;

        CreateSystem();*/
    }

    public void Recreate()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Randomize();
    }
}
