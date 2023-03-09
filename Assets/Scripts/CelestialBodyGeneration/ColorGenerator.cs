using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGenerator
{
    ColorSettings settings;
    Texture2D texture;
    const int TEXTURE_RESOLUTION = 50;
    INoiseFilter biomeNoiseFilter;
    MeshRenderer[] meshFilters;

    public void UpdateSettings(ColorSettings colorSettings, MeshRenderer[] meshRenderer)
    {
        this.settings = colorSettings;

        if (texture == null || texture.height != settings.biomeColorSettings.biomes.Length)
            texture = new Texture2D(TEXTURE_RESOLUTION * 2, settings.biomeColorSettings.biomes.Length, TextureFormat.RGBA32, false);

        biomeNoiseFilter = NoiseFilterFactory.CreateNoiseFilter(settings.biomeColorSettings.noise);

        this.meshFilters = meshRenderer;
    }

    public void UpdateElevation(MinMax elevationMinMax)
    {
        foreach(var meshFilter in meshFilters)
        {
            meshFilter.material.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
        }
    }

    public float BiomePercentFromtPoint(Vector3 pointOnUnitSphere)
    {
        float heightPercent = (pointOnUnitSphere.y + 1) / 2f;
        heightPercent += (biomeNoiseFilter.Evaluate(pointOnUnitSphere) - settings.biomeColorSettings.noiseOffset) * settings.biomeColorSettings.noiseStrength;
        float biomeIndex = 0;
        int numBiomes = settings.biomeColorSettings.biomes.Length;

        float blendRange = settings.biomeColorSettings.blendAmount / 2f + 0.001f;

        for (int i = 0; i < numBiomes; i++)
        {
            float dst = heightPercent - settings.biomeColorSettings.biomes[i].startHeight;
            float weight = Mathf.InverseLerp(-blendRange, blendRange, dst);
            biomeIndex *= (1 - weight);
            biomeIndex += i * weight;
        }

        return biomeIndex / Mathf.Max(1, numBiomes - 1);
    }

    public void UpdateColors()
    {
        Color[] colors = new Color[texture.width * texture.height];
        int colorIndex = 0;
        foreach (var biome in settings.biomeColorSettings.biomes)
        {
            for (int i = 0; i < TEXTURE_RESOLUTION * 2; i++)
            {
                Color gradientColor;
                if (i < TEXTURE_RESOLUTION)
                {
                    gradientColor = settings.oceanColor.Evaluate(i / (TEXTURE_RESOLUTION - 1f));
                }
                else
                {
                    gradientColor = biome.gradient.Evaluate((i - TEXTURE_RESOLUTION) / (TEXTURE_RESOLUTION - 1f));
                }
                Color tintCol = biome.tint;
                colors[colorIndex++] = gradientColor * (1 - biome.tintPercent) + tintCol * biome.tintPercent;
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
        foreach (var meshFilter in meshFilters)
        {
            meshFilter.material.SetTexture("_texture", texture);
        }
    }
}
