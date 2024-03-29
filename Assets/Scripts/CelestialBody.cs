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

    public Mesh[] precalculatedMeshes;
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public ShapeGenerator shapeGenerator = new ShapeGenerator();
    public ColorGenerator colorGenerator = new ColorGenerator();

    GameObject meshesGo;

    public static List<ShapeGenerator> shapeGenerators = new List<ShapeGenerator>();

    int id;

    private void Start()
    {
        id = shapeGenerators.Count;
        shapeGenerators.Add(shapeGenerator);
        Recreate();
    }

    [ContextMenu("Recreate")]
    public void Recreate()
    {
        Initialize();
        GenerateMesh();
        GenerateColors();
        SetLOD(0);
    }

    void Initialize()
    {
        shapeGenerator.UpdateSettings(shapeSettings);
        meshesGo = new GameObject();
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
                //meshRenderer.material.EnableKeyword("_EMISSION");
                meshRenderer.material.SetColor("_EmissionColor", data.color * 2);
            }
            else
            {
                meshRenderer.material.SetFloat("_smoothness", data.waterAlbedo);
                meshRenderer.material.SetFloat("_LandSmoothness", data.albedo);
            }
            meshRenderers[i] = meshRenderer;
            meshFilters[i] = meshObj.AddComponent<MeshFilter>();
            meshFilters[i].mesh = new Mesh();
            var meshCollider = meshObj.AddComponent<MeshCollider>();
            meshCollider.convex = true;

            terrainFaces[i] = new TerrainFace(meshFilters[i], meshCollider, resolution, directions[i], shapeGenerator, id);
        }
        colorGenerator.UpdateSettings(colorSettings, meshRenderers);

        if (data.type == CelestialBodyType.Star)
        {
            var light = gameObject.AddComponent<Light>();
            light.useColorTemperature = true;
            light.colorTemperature = data.temperature * 100f;
            light.type = LightType.Point;
            light.range = data.lightRange;
            light.intensity = data.lightIntensity;
        }
    }

    private void Update()
    {
        var lod = LODManager.instance.GetLODLevel(transform.position);

        SetLOD(lod);

        if (lod >= LODManager.instance.lodLevels.Length - 1) return;

        if (!data.tidalLocked)
            meshesGo.transform.Rotate(data.rotationAxis, data.rotationSpeed * Time.deltaTime, Space.Self);
        else
            meshesGo.transform.LookAt(transform.parent.position);
        transform.RotateAround(transform.parent.position, data.orbitAxis, data.orbitSpeed * Time.deltaTime);
    }

    void GenerateMesh()
    {
        foreach(var face in terrainFaces)
        {
            face.ConstructMeshes();
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

    public void SetLOD(int lod)
    {
        foreach(var face in terrainFaces)
        {
            face.SetLOD(lod);
        }
    }
}

[System.Serializable]
public class CelestialBodyData
{
    public string name;

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

    public float distanceFromStar;

    public float waterAlbedo;

    public float albedo;

    public float temperature;
}

public enum CelestialBodyType
{
    Planet,
    GasGiant,
    Star
}
