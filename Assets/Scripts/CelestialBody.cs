using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public CelestialBodyData data;

    public Material material;

    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;

    public int resolution;

    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public ShapeGenerator shapeGenerator = new ShapeGenerator();
    public ColorGenerator colorGenerator = new ColorGenerator();

    private void Start()
    {
        Initialize();
        GenerateMesh();
        GenerateColors();
    }

    void Initialize()
    {
        shapeGenerator.UpdateSettings(shapeSettings);
        GameObject meshesGo = new GameObject();
        meshesGo.name = "Graphics";
        meshesGo.transform.parent = transform;
        meshesGo.transform.localPosition = new Vector3(0, 0, 0);

        meshFilters = new MeshFilter[6];
        terrainFaces = new TerrainFace[6];
        var meshRenderers = new MeshRenderer[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            GameObject meshObj = new GameObject("Mesh");
            meshObj.transform.parent = meshesGo.transform;
            meshObj.transform.localPosition = Vector3.zero;

            var meshRenderer = meshObj.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            if (data.type == CelestialBodyType.Star)
            {
                meshRenderer.material.EnableKeyword("_EMISSION");
                meshRenderer.material.SetColor("_EmissionColor", data.color);
            }
            meshRenderers[i] = meshRenderer;
            meshFilters[i] = meshObj.AddComponent<MeshFilter>();
            meshFilters[i].mesh = new Mesh();

            terrainFaces[i] = new TerrainFace(meshFilters[i].mesh, resolution, directions[i], shapeGenerator);
        }
        colorGenerator.UpdateSettings(colorSettings, meshRenderers);

        if (data.type == CelestialBodyType.Star)
        {
            GameObject lightGo = new GameObject();
            lightGo.transform.parent = transform;
            var light = lightGo.AddComponent<Light>();
            light.color = Color.Lerp(Color.white, data.color, 0.75f);
            light.type = LightType.Point;
            light.range = data.lightRange;
            light.intensity = data.lightIntensity;
        }
    }

    private void Update()
    {
        if (!data.tidalLocked)
            transform.Rotate(data.rotationAxis, data.rotationSpeed * Time.deltaTime, Space.Self);
        else
            transform.LookAt(transform.parent.position);
        transform.RotateAround(transform.parent.position, data.orbitAxis, data.orbitSpeed * Time.deltaTime);
    }

    void GenerateMesh()
    {
        foreach(var face in terrainFaces)
        {
            face.ConstructMesh();
        }

        colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColors()
    {
        colorGenerator.UpdateColors();
        for (int i = 0; i < terrainFaces.Length; i++)
        {
                terrainFaces[i].UpdateUVs(colorGenerator);
        }
    }
}

[System.Serializable]
public class CelestialBodyData
{
    public CelestialBodyType type;

    public float orbitDistance;

    public float radius;

    public float rotationSpeed;

    public Vector3 rotationAxis;

    public float orbitSpeed;

    public Vector3 orbitAxis;

    public float initialOrbitProgress;

    [ColorUsage(false, false)]
    public Color color;

    public CelestialBodyData[] orbitingBodies;

    public float lightRange;

    public float lightIntensity;

    public bool tidalLocked;
}

public enum CelestialBodyType
{
    Planet,
    GasGiant,
    Star
}
