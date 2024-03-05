using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public Renderer textureRenderer;

    // This can be set to true in the Inspector to automatically re-render the map on any change.
    public bool autoUpdate;

    // Variables for noise map colors.
    public Color deepOceanColor;
    public Color oceanColor;
    public Color lowOceanColor;
    public Color beachColor;
    public Color lightGroundColor;
    public Color groundColor;
    public Color mountainColor;
    public Color snowColor;
    public Color cityColor;

    // Map settings.
    [Header("General Settings")]
    public int mapWidth;
    public int mapHeight;
    public Vector2 offset;

    // See documentation for the different noise settings in the Noise.cs file.
    // Noise settings, scales and a range slider to limit the values.
    [Header("Noise Settings")]
    public float noiseScale;
    public int octaves;
    [Range(0, 1)] public float persistence;
    public float lacunarity;
    public int seed;

    // Same settings for generating the city noise map
    [Header("City noise Settings")]
    public float cityNoiseScale;
    public int cityOctaves;
    [Range(0, 1)] public float cityPersistence;
    public float cityLacunarity;
    public int citySeed;

    [Header("Color limits")]
    // Range sliders for noise map values.
    [Range(0, 1)] public float oceanLimit;
    [Range(0, 1)] public float lowOceanLimit;
    [Range(0, 1)] public float beachLimit;
    [Range(0, 1)] public float lightGroundLimit;
    [Range(0, 1)] public float groundLimit;
    [Range(0, 1)] public float mountainLimit;
    [Range(0, 1)] public float snowLimit;
    [Range(0, 1)] public float cityLimit;

    // Define limit and color pairs for cleaner logic
    public List<KeyValuePair<float, Color>> colorRanges = new List<KeyValuePair<float, Color>>();

    /// <summary>
    /// Generates the noise map.
    /// </summary>
    public void GenerateMap()
    {
        // If you want to generate additional noisemaps, you can call the function many times with randomized seeds and different options.
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistence, lacunarity, offset);
        // Another noise map for for city generation
        float[,] cityNoiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, citySeed, cityNoiseScale, cityOctaves, cityPersistence, cityLacunarity, offset);

        // This draws the map (so don't remove it).
        DrawNoiseMap(noiseMap, cityNoiseMap);
    }

    /// <summary>
    /// There are used to clamp values in the inspector, since they break some parts of the map.
    /// </summary>
    void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }

    /// <summary>
    /// Draws the noise map from a two dimensional array.
    /// </summary>
    /// <param name="noiseMap">Generated noise map</param>
    public void DrawNoiseMap(float[,] noiseMap, float[,] cityNoiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];

        // Add all the limit color pairs to the List
        colorRanges.Add(new KeyValuePair<float, Color>(snowLimit, snowColor));
        colorRanges.Add(new KeyValuePair<float, Color>(mountainLimit, mountainColor));
        colorRanges.Add(new KeyValuePair<float, Color>(groundLimit, groundColor));
        colorRanges.Add(new KeyValuePair<float, Color>(lightGroundLimit, lightGroundColor));
        colorRanges.Add(new KeyValuePair<float, Color>(beachLimit, beachColor));
        colorRanges.Add(new KeyValuePair<float, Color>(lowOceanLimit, lowOceanColor));
        colorRanges.Add(new KeyValuePair<float, Color>(oceanLimit, oceanColor));
        colorRanges.Add(new KeyValuePair<float, Color>(0, deepOceanColor));

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // This is how you get the value at a location in the noise map.
                float noise = noiseMap[x, y];
                float cityNoise = cityNoiseMap[x, y];

                // Here we set the colours of each pixel based on their noise map's value:
                // Find the first color range where noise falls within the limit and use that color
                Color chosenColor = colorRanges.Find(range => noise > range.Key).Value;
                colourMap[y * width + x] = chosenColor;

                // Cities appear between ocean and ground limits
                // Use the separate cityNoiseMap to generate the locations in these limits
                if (noise > beachLimit && noise < groundLimit && cityNoise > cityLimit) {
                    colourMap[y * width + x] = cityColor;
                }
            }
        }

        // These just set colors to the texture and apply it. No need to touch these.
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(width * 0.1f, 1f, height * 0.1f);
    }
}
