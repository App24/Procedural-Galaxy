using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

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

    [Delayed, Min(1)]
    public int maxPlanets = 10;

    public Material planetMaterial;

    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;

    public Gradient dryOceanColor;
    public Gradient wetOceanColor;
    public Gradient frozenOceanColor;

    public Material trailMaterial;

    const string glyphs = "0123456789";

    private void Awake()
    {
        //Randomize();
    }

    void Randomize()
    {
        Random.InitState(seed);
        data = new SolarSystemData();
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
        data.sunBody.orbitAxis = Vector3.up;
        if (Random.value < 0.2f) data.sunBody.orbitAxis = Vector3.down;

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
            if(Random.value < 0.1f)
            {
                var moon = new CelestialBodyData();
                moon.name = $"(Moon) {planetName}.1";
                moon.type = CelestialBodyType.Planet;
                moon.orbitDistance = Random.Range(body.radius * 1.5f, body.radius * 2.5f);
                moon.orbitingBodies = new CelestialBodyData[0];
                moon.rotationSpeed = Random.Range(20f, 50f);
                moon.rotationAxis = Vector3.up;
                moon.radius = Random.Range(body.radius/20f, body.radius/5f);
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
            body.albedo = Mathf.Clamp01(body.distanceFromStar - 0.5f)  * 2 * 0.5f;
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
        go.name = body.name;
        var celestialBody = go.AddComponent<CelestialBody>();
        celestialBody.material = planetMaterial;
        celestialBody.data = body;
        celestialBody.resolution = body.type == CelestialBodyType.Planet ? celestialBodyResolution : 20;
        var shapeSettings = ShapeSettings.CreateInstance<ShapeSettings>();
        shapeSettings.planetRadius = body.radius;
        shapeSettings.noiseLayers = new ShapeSettings.NoiseLayer[0];
        var colorSettings = ColorSettings.CreateInstance<ColorSettings>();
            colorSettings.oceanColor = this.colorSettings.oceanColor.CloneGradient();
        {
            colorSettings.biomeColorSettings = new ColorSettings.BiomeColorSettings();
            colorSettings.biomeColorSettings.biomes = new ColorSettings.BiomeColorSettings.Biome[this.colorSettings.biomeColorSettings.biomes.Length];
            for (int i = 0; i < colorSettings.biomeColorSettings.biomes.Length; i++)
            {
                var biome = new ColorSettings.BiomeColorSettings.Biome();
                biome.gradient = this.colorSettings.biomeColorSettings.biomes[i].gradient.CloneGradient();
                biome.tint = this.colorSettings.biomeColorSettings.biomes[i].tint.Copy();
                biome.startHeight = this.colorSettings.biomeColorSettings.biomes[i].startHeight;
                biome.tintPercent = this.colorSettings.biomeColorSettings.biomes[i].tintPercent;
                colorSettings.biomeColorSettings.biomes[i] = biome;
            }
            colorSettings.biomeColorSettings.noiseStrength = this.colorSettings.biomeColorSettings.noiseOffset;
            colorSettings.biomeColorSettings.noise = this.colorSettings.biomeColorSettings.noise.Copy();
            colorSettings.biomeColorSettings.noiseStrength = this.colorSettings.biomeColorSettings.noiseStrength;
            colorSettings.biomeColorSettings.blendAmount = this.colorSettings.biomeColorSettings.blendAmount;
        }
        if (body.type == CelestialBodyType.Planet)
        {
            {
                shapeSettings.noiseLayers = new ShapeSettings.NoiseLayer[this.shapeSettings.noiseLayers.Length];
                for (int i = 0; i < shapeSettings.noiseLayers.Length; i++)
                {
                    shapeSettings.noiseLayers[i] = this.shapeSettings.noiseLayers[i].Copy();
                    {
                        shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.center = Random.insideUnitSphere * Random.Range(0, 5000f);
                        shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.baseRoughness += Random.Range(-0.5f, 0.5f);
                        shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.minValue += Random.Range(-0.05f, 0.05f);
                        shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.persistence += Random.Range(-0.05f, 0.05f);
                        shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.roughness += Random.Range(-0.05f, 0.05f);
                        shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.strength += Random.Range(-0.05f, 0.05f);
                    }
                    {
                        shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.center = Random.insideUnitSphere * Random.Range(0, 5000f);
                        shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.baseRoughness += Random.Range(-0.5f, 0.5f);
                        shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.minValue += Random.Range(-0.05f, 0.05f);
                        shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.persistence += Random.Range(-0.05f, 0.05f);
                        shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.roughness += Random.Range(-0.05f, 0.05f);
                        shapeSettings.noiseLayers[i].noiseSettings.ridgidNoiseSettings.strength += Random.Range(-0.05f, 0.05f);
                    }
                }
            }
            {
                if (body.distanceFromStar <= 0.5f)
                {
                    var lerpValue = body.distanceFromStar * 2f;
                    colorSettings.oceanColor = UnityUtilities.Lerp(dryOceanColor, wetOceanColor, lerpValue);
                    foreach (var biome in colorSettings.biomeColorSettings.biomes)
                    {
                        biome.gradient = UnityUtilities.Lerp(new Gradient()
                        {
                            colorKeys = new GradientColorKey[]
                            {
                                new GradientColorKey(Color.gray, 0),
                                new GradientColorKey(Color.gray, 1),
                            },
                            alphaKeys = new GradientAlphaKey[]
                            {
                                new GradientAlphaKey(1, 0),
                                new GradientAlphaKey(1, 1),
                            },
                            mode = GradientMode.Blend
                        }, biome.gradient, lerpValue);
                    }
                    /*colorSettings.oceanColor.colorKeys = new GradientColorKey[]
                    {
                        new GradientColorKey(Color.Lerp(new Color(), new Color(0f, 0.1538353f, 1f), body.distanceFromStar * 2f), 0),
                        new GradientColorKey(Color.Lerp(new Color(), new Color(0.2688679f, 0.6051331f, 1f), body.distanceFromStar * 2f), 0.8f),
                        new GradientColorKey(Color.Lerp(new Color(), new Color(0f, 0.9338689f, 1f), body.distanceFromStar * 2f), 1),
                    };*/
                }
                else
                {
                    var lerpValue = (body.distanceFromStar - 0.5f) * 2f;
                    colorSettings.oceanColor = UnityUtilities.Lerp(wetOceanColor, frozenOceanColor, lerpValue);
                    foreach (var biome in colorSettings.biomeColorSettings.biomes)
                    {
                        biome.gradient = UnityUtilities.Lerp(biome.gradient, new Gradient()
                        {
                            colorKeys = new GradientColorKey[]
                            {
                                new GradientColorKey(Color.white, 0),
                                new GradientColorKey(Color.white, 1),
                            },
                            alphaKeys = new GradientAlphaKey[]
                            {
                                new GradientAlphaKey(1, 0),
                                new GradientAlphaKey(1, 1),
                            },
                            mode = GradientMode.Blend
                        }, lerpValue);
                    }
                }
            }
        }
        celestialBody.shapeSettings = shapeSettings;
        celestialBody.colorSettings = colorSettings;
        go.transform.parent = parent;
        go.transform.localPosition = new Vector3(0, 0, distance);
        go.transform.RotateAround(parent.position, body.orbitAxis, 360f * body.initialOrbitProgress);

        /*var trailRenderer = go.AddComponent<TrailRenderer>();
        trailRenderer.material = trailMaterial;
        trailRenderer.generateLightingData = false;
        trailRenderer.time = float.MaxValue;*/

        if (body.radius > 1.5f)
        {
            var gravityAttractorGo = new GameObject();
            gravityAttractorGo.transform.SetParent(go.transform, false);
            /*if (body.type == CelestialBodyType.Star)
            {
                gravityAttractorGo.transform.localScale = body.lightRange * Vector3.one;
            }
            else
            {
                gravityAttractorGo.transform.localScale = body.radius * Vector3.one * 4f;
            }*/
            var collider = gravityAttractorGo.AddComponent<SphereCollider>();
            collider.radius = body.radius * 4f;
            if (body.type == CelestialBodyType.Star)
            {
                collider.radius = body.lightRange;
            }
            collider.isTrigger = true;

            gravityAttractorGo.AddComponent<GravityAttractor>();
        }

        foreach (var child in body.orbitingBodies)
        {
            CreateBody(child.orbitDistance, go.transform, child);
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

    /*private void OnValidate()
    {
        if (!Application.isPlaying) return;
        Recreate();
    }*/
}

[System.Serializable]
public class SolarSystemData
{
    public CelestialBodyData sunBody;
}
