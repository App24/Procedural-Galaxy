using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    ShapeGenerator shapeGenerator;
    MeshFilter meshFilter;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    Mesh[] lodMeshes;

    int currentLOD = -1;

    public TerrainFace(MeshFilter meshFilter, int resolution, Vector3 localUp, ShapeGenerator shapeGenerator)
    {
        this.meshFilter = meshFilter;
        this.resolution = resolution;
        this.localUp = localUp;
        this.shapeGenerator = shapeGenerator;
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
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
        for (int j = 0; j < lodMeshes.Length - 1; j++)
        {
            var mesh = lodMeshes[j];
            var lodResolution = resolution / (j + 1);
            Vector3[] vertices = new Vector3[lodResolution * lodResolution];
            Vector3[] normals = new Vector3[lodResolution * lodResolution];
            int[] triangles = new int[(lodResolution - 1) * (lodResolution - 1) * 6];
            int triIndex = 0;
            Vector2[] uv = (mesh.uv.Length == vertices.Length) ? mesh.uv : new Vector2[vertices.Length];

            for (int y = 0; y < lodResolution; y++)
            {
                for (int x = 0; x < lodResolution; x++)
                {
                    int i = x + y * lodResolution;
                    Vector2 percent = new Vector2(x, y) / (lodResolution - 1);
                    Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    float unscaledElevation = shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);
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
