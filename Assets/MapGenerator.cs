using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Renderer textureRenderer;

    // This can be set to true in the Inspector to automatically re-render the map on any change.
    public bool autoUpdate;

    // Variables for noise map colors.
    public Color groundColor;
    public Color oceanColor;
    public Color beachColor;
    public Color mountainColor;
    public Color snowColor;

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

    // Range sliders for noise map values.
    [Range(0, 1)] public float beachLimit;
    [Range(0, 1)] public float groundLimit;
    [Range(0, 1)] public float mountainLimit;
    [Range(0, 1)] public float snowLimit;

    /// <summary>
    /// Generates the noise map.
    /// </summary>
    public void GenerateMap()
    {
        // If you want to generate additional noisemaps, you can call the function many times with randomized seeds and different options.
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistence, lacunarity, offset);

        // This draws the map (so don't remove it).
        DrawNoiseMap(noiseMap);
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
    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // This is how you get the value at a location in the noise map.
                float noise = noiseMap[x, y];

                // Here we set the colours of each pixel based on their noise map's value.
                if (noise > snowLimit) {
                    colourMap[y * width + x] = snowColor;
                } else if (noise > mountainLimit) {
                    colourMap[y * width + x] = mountainColor;
                } else if (noise > groundLimit) {
                    colourMap[y * width + x] = groundColor;
                } else if (noise > beachLimit) {
                    colourMap[y * width + x] = beachColor;
                } else {
                    colourMap[y * width + x] = oceanColor;
                }
                
                // You can add if-else clauses or other bits of logic to add
                // different colors based on the noise map's value.


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
