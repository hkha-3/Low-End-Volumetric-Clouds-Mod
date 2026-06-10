using System;
using System.IO;
using UnityEngine;

namespace LowEndVolumetricCloudsMod
{
    public static class WeatherMapGenerator
    {
        public static void GenerateDefaultMap(string savePath)
        {
            int size = 512; // A highly optimized resolution square for low-end graphics cards
            Texture2D weatherTex = new Texture2D(size, size, TextureFormat.RGB24, false);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Convert pixel loops into decimal coordinates
                    float xCoord = (float)x / size * 8.0f;
                    float yCoord = (float)y / size * 8.0f;

                    // Generate procedural math vectors using Unity's built-in Perlin Noise
                    float redChannel   = Mathf.PerlinNoise(xCoord, yCoord);               // General Density
                    float greenChannel = Mathf.PerlinNoise(xCoord * 2.0f, yCoord * 2.0f); // Cavities & Canyons
                    float blueChannel  = Mathf.PerlinNoise(xCoord * 0.5f, yCoord * 0.5f); // Storm Cyclone Swirls

                    // Pack the procedural noise into the texture data array
                    pixels[x + y * size] = new Color(redChannel, greenChannel, blueChannel);
                }
            }

            weatherTex.SetPixels(pixels);
            weatherTex.Apply();

            // Encode the data array into a standard physical .png image asset
            byte[] bytes = weatherTex.EncodeToPNG();
            File.WriteAllBytes(savePath, bytes);
            UnityEngine.Object.DestroyImmediate(weatherTex);
            
            Debug.Log($"[LowEndVolumetricCloudsMod] Successfully baked default weather map texture to: {savePath}");
        }
    }
}
