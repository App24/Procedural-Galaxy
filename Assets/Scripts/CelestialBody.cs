using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public CelestialBodyData data;

    public int resolution;

    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    private void Start()
    {
        Initialize();
        GenerateMesh();
    }

    void Initialize()
    {
        GameObject meshesGo = new GameObject();
        meshesGo.name = "Graphics";
        meshesGo.transform.parent = transform;
        meshesGo.transform.localPosition = new Vector3(0, 0, 0);

        meshFilters = new MeshFilter[6];
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            GameObject meshObj = new GameObject("Mesh");
            meshObj.transform.parent = meshesGo.transform;
            meshObj.transform.localPosition = Vector3.zero;

            var meshRenderer = meshObj.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (data.type == CelestialBodyType.Star)
            {
                meshRenderer.material.EnableKeyword("_EMISSION");
                meshRenderer.material.SetColor("_EmissionColor", data.color);
            }
                meshRenderer.material.SetColor("_BaseColor", data.color);
            meshFilters[i] = meshObj.AddComponent<MeshFilter>();
            meshFilters[i].mesh = new Mesh();

            terrainFaces[i] = new TerrainFace(meshFilters[i].mesh, resolution, directions[i], data.radius);
        }

        if(data.type == CelestialBodyType.Star)
        {
            GameObject lightGo = new GameObject();
            lightGo.transform.parent = transform;
            var light = lightGo.AddComponent<Light>();
            light.color = data.color;
            light.type = LightType.Point;
            light.range = data.radius * 10f;
            light.intensity = 500;
        }
    }

    void GenerateMesh()
    {
        foreach(var face in terrainFaces)
        {
            face.ConstructMesh();
        }
    }
}

[System.Serializable]
public class CelestialBodyData
{
    public CelestialBodyType type;

    public float orbitDistance;

    public float radius;

    [ColorUsage(false, false)]
    public Color color;

    public CelestialBodyData[] orbitingBodies;
}

public enum CelestialBodyType
{
    Planet,
    GasGiant,
    Star
}
