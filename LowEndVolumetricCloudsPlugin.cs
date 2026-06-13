using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace LowEndVolumetricCloudsMod
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class LowEndVolumetricCloudsPlugin : MonoBehaviour
    {
        private Camera flightCamera;
        private CommandBuffer cloudCommandBuffer;
        private Material cloudMaterial;
        private Texture2D loadedWeatherMap;
        
        private bool isPipelineInitialized = false;
        
        // PERFORMANCE TUNING: 4 means clouds render at 1/4 resolution (e.g. 1080p -> 270p)
        private const int DOWNSCALE_FACTOR = 4;

        void Start()
        {
            Debug.Log("[LowEndVolumetricCloudsMod] Start - preparing textures...");
            InitializeAssets();
        }

        void Update()
        {
            if (!isPipelineInitialized)
            {
                TryInitializePipeline();
            }

            if (!isPipelineInitialized || cloudMaterial == null || FlightGlobals.currentMainBody == null) return;

            // DYNAMIC PLANET DETECTION: Read what planet the player is orbiting
            string planetName = FlightGlobals.currentMainBody.name;

            if (planetName == "Kerbin")
            {
                // Kerbin Profile: Crisp, fluffy white/grey storm decks
                cloudMaterial.SetColor("_CloudColor", new Color(0.92f, 0.92f, 0.95f, 1.0f)); 
                cloudMaterial.SetFloat("_IntensityModifier", 0.65f); 
            }
            else if (planetName == "Jool")
            {
                // Jool Profile: Thick, heavy cinematic gas-giant green
                cloudMaterial.SetColor("_CloudColor", new Color(0.12f, 0.58f, 0.28f, 1.0f)); 
                cloudMaterial.SetFloat("_IntensityModifier", 0.90f); 
            }
            else
            {
                // Turn clouds completely off if the planet has no atmosphere (like the Mun or Minmus)
                cloudMaterial.SetColor("_CloudColor", new Color(0, 0, 0, 0));
                cloudMaterial.SetFloat("_IntensityModifier", 0.0f);
            }
        }

        private void InitializeAssets()
        {
            // 1. Set up the local folders and ensure the weather map texture exists on disk
            string texturesDirectory = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/LowEndVolumetricCloudsMod/Textures");
            string mapPath = Path.Combine(texturesDirectory, "weather_map.png");

            try
            {
                if (!Directory.Exists(texturesDirectory))
                {
                    Directory.CreateDirectory(texturesDirectory);
                }

                // If the user doesn't have a weather map image, generate a seamless default one automatically
                if (!File.Exists(mapPath))
                {
                    WeatherMapGenerator.GenerateDefaultMap(mapPath);
                }

                // Load the texture directly from the game folder files
                byte[] fileData = File.ReadAllBytes(mapPath);
                loadedWeatherMap = new Texture2D(2, 2, TextureFormat.RGB24, false);
                loadedWeatherMap.LoadImage(fileData);
                loadedWeatherMap.filterMode = FilterMode.Bilinear;
                loadedWeatherMap.wrapMode = TextureWrapMode.Repeat;
                Debug.Log("[LowEndVolumetricCloudsMod] Weather map texture loaded successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LowEndVolumetricCloudsMod] Critical error loading weather texture: {ex.Message}");
            }
        }

        private void TryInitializePipeline()
        {
            // Try to resolve the active flight camera
            flightCamera = FlightCamera.fetch?.mainCamera;
            if (flightCamera == null)
            {
                flightCamera = Camera.main; // Fallback to Unity's main camera if KSP's fetch singleton isn't fully ready yet
            }

            if (flightCamera == null)
            {
                // Not ready yet, retry on next frame
                return;
            }

            // Locate and load custom shader asset
            Shader cloudShader = Shader.Find("Hidden/LowEndVolumetricCloud");
            if (cloudShader == null)
            {
                Debug.LogWarning("[LowEndVolumetricCloudsMod] Warning: Hidden/LowEndVolumetricCloud shader not found. Searching fallback...");
                cloudShader = Shader.Find("Sprites/Default");
            }
            
            cloudMaterial = new Material(cloudShader);

            // Feed the loaded texture image straight into the shader's internal properties
            if (loadedWeatherMap != null)
            {
                cloudMaterial.SetTexture("_WeatherMap", loadedWeatherMap);
            }

            // Inject the performance-saving graphics buffer loops
            InitializeCommandBuffer();
            isPipelineInitialized = true;
            Debug.Log("[LowEndVolumetricCloudsMod] Graphics pipeline successfully initialized!");
        }

        void InitializeCommandBuffer()
        {
            if (flightCamera == null) return;

            cloudCommandBuffer = new CommandBuffer();
            cloudCommandBuffer.name = "LowEndVolumetricClouds";

            int targetWidth = Math.Max(1, Screen.width / DOWNSCALE_FACTOR);
            int targetHeight = Math.Max(1, Screen.height / DOWNSCALE_FACTOR);

            int lowResBufferID = Shader.PropertyToID("_LowResCloudTex");
            cloudCommandBuffer.GetTemporaryRT(lowResBufferID, targetWidth, targetHeight, 0, FilterMode.Bilinear);

            cloudCommandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, lowResBufferID, cloudMaterial);
            cloudCommandBuffer.Blit(lowResBufferID, BuiltinRenderTextureType.CurrentActive);
            cloudCommandBuffer.ReleaseTemporaryRT(lowResBufferID);

            flightCamera.AddCommandBuffer(CameraEvent.AfterSkybox, cloudCommandBuffer);
            Debug.Log($"[LowEndVolumetricCloudsMod] Command buffer injected into flight camera ({targetWidth}x{targetHeight}).");
        }

        void OnDestroy()
        {
            // Clear out graphic pipelines to eliminate memory leaks and runtime crash vectors
            if (flightCamera != null && cloudCommandBuffer != null)
            {
                flightCamera.RemoveCommandBuffer(CameraEvent.AfterSkybox, cloudCommandBuffer);
            }

            if (cloudMaterial != null)
            {
                Destroy(cloudMaterial);
            }

            if (loadedWeatherMap != null)
            {
                Destroy(loadedWeatherMap);
            }

            Debug.Log("[LowEndVolumetricCloudsMod] Graphics pipeline shut down safely.");
        }
    }
}
