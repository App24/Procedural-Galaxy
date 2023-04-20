using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class TerrainFace
{
    ShapeGenerator shapeGenerator;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    Mesh[] lodMeshes;

    int currentLOD = -1;

    int id;

    public TerrainFace(MeshFilter meshFilter, MeshCollider meshCollider, int resolution, Vector3 localUp, ShapeGenerator shapeGenerator, int id)
    {
        this.meshFilter = meshFilter;
        this.resolution = resolution;
        this.localUp = localUp;
        this.shapeGenerator = shapeGenerator;
        this.meshCollider = meshCollider;
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
        this.id = id;
    }

    public void ConstructMeshes()
    {
        if(lodMeshes == null)
        {
            lodMeshes = new Mesh[LODManager.instance.lodLevels.Length];
            for (int i = 0; i < lodMeshes.Length; i++)
            {
                lodMeshes[i] = new Mesh();
            }
        }

        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(lodMeshes.Length - 1, Allocator.Temp);
        CalculateMeshData[] jobs = new CalculateMeshData[lodMeshes.Length - 1];

        for (int j = 0; j < lodMeshes.Length - 1; j++)
        {
            var lodResolution = resolution / (j + 1);
            CalculateMeshData calculateMeshData = new CalculateMeshData();

            calculateMeshData.axisA = axisA;
            calculateMeshData.axisB = axisB;
            calculateMeshData.localUp = localUp;
            calculateMeshData.resolution = lodResolution;
            calculateMeshData.id = id;
            calculateMeshData.points = new NativeArray<Vector3>(lodResolution * lodResolution, Allocator.TempJob);
            calculateMeshData.values = new NativeArray<float>(lodResolution * lodResolution, Allocator.TempJob);

            jobs[j] = calculateMeshData;
            jobHandles[j] = calculateMeshData.Schedule();
        }

        JobHandle.CompleteAll(jobHandles);

        jobHandles.Dispose();

        for (int j = 0; j < lodMeshes.Length - 1; j++)
        {
            var mesh = lodMeshes[j];
            var lodResolution = resolution / (j + 1);
            Vector3[] vertices = new Vector3[lodResolution * lodResolution];
            Vector3[] normals = new Vector3[lodResolution * lodResolution];
            int[] triangles = new int[(lodResolution - 1) * (lodResolution - 1) * 6];
            int triIndex = 0;
            Vector2[] uv = (mesh.uv.Length == vertices.Length) ? mesh.uv : new Vector2[vertices.Length];

            var job = jobs[j];

            for (int y = 0; y < lodResolution; y++)
            {
                for (int x = 0; x < lodResolution; x++)
                {
                    int i = x + y * lodResolution;
                    Vector2 percent = new Vector2(x, y) / (lodResolution - 1);
                    Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    float unscaledElevation = 0;
                    if (job.points[i] == pointOnUnitSphere)
                    {
                        unscaledElevation = job.values[i];
                    }
                    else
                    {
                        unscaledElevation = shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);
                    }
                    vertices[i] = pointOnUnitSphere * shapeGenerator.GetScaledElevation(unscaledElevation);
                    normals[i] = vertices[i];
                    uv[i].y = unscaledElevation;

                    if (x != lodResolution - 1 && y != lodResolution - 1)
                    {
                        triangles[triIndex++] = i;
                        triangles[triIndex++] = i + lodResolution + 1;
                        triangles[triIndex++] = i + lodResolution;

                        triangles[triIndex++] = i;
                        triangles[triIndex++] = i + 1;
                        triangles[triIndex++] = i + lodResolution + 1;
                    }
                }
            }

            job.points.Dispose();
            job.values.Dispose();

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            //mesh.RecalculateNormals();
            mesh.uv = uv;
        }
    }

    public void SetLOD(int lod)
    {
        if (lod == currentLOD) return;

        if(lod < 0 || lod >= lodMeshes.Length) return;

        currentLOD = lod;
        meshFilter.mesh = lodMeshes[lod];
        meshCollider.sharedMesh = lodMeshes[lodMeshes.Length - 1];
    }

    public void UpdateUVs(ColorGenerator colorGenerator)
    {
        for (int j = 0; j < lodMeshes.Length - 1; j++)
        {
            var mesh = lodMeshes[j];
            var lodResolution = resolution / (j + 1);
            Vector2[] uv = mesh.uv;
            for (int y = 0; y < lodResolution; y++)
            {
                for (int x = 0; x < lodResolution; x++)
                {
                    int i = x + y * lodResolution;
                    Vector2 percent = new Vector2(x, y) / (lodResolution - 1);
                    Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                    uv[i].x = colorGenerator.BiomePercentFromtPoint(pointOnUnitSphere);
                }
            }

            mesh.uv = uv;
        }
    }
}

public struct CalculateMeshData : IJob
{
    [ReadOnly]
    public int resolution;

    [ReadOnly]
    public Vector3 axisA, axisB, localUp;

    [ReadOnly]
    public int id;

    [WriteOnly]
    public NativeArray<Vector3> points;

    [WriteOnly]
    public NativeArray<float> values;

    public void Execute()
    {
        var shapeGenerator = CelestialBody.shapeGenerators[id];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                float unscaledElevation = shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);
                points[i] = pointOnUnitSphere;
                values[i] = unscaledElevation;
            }
        }
    }
}