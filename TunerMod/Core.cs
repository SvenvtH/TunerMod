using System.Collections;
using System.IO;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using UnityEngine.Experimental.Rendering;

[assembly: MelonInfo(typeof(TunerMod.Core), "TunerMod", "1.0.0", "RedReflex", null)]
[assembly: MelonGame("TVGS", "Schedule I")]
namespace TunerMod
{
    public class Core : MelonMod
    {
        private string assetBundlePath = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods", "Assets", "spoiler.asset");
        private string modelName = "SpoilerModel";

        private GameObject spawnedModel;
        private Il2CppAssetBundle loadedBundle;
        private bool modelSpawned = false;

        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Asset Bundle Spawner Mod initialized!");
            MelonLogger.Msg($"Will attempt to load bundle from: {assetBundlePath}");
        }

        public override void OnUpdate()
        {
            // Press F9 to load and spawn the model
            if (Input.GetKeyDown(KeyCode.F9) && !modelSpawned)
            {
                LoadAndSpawnModel();
            }

            // Press F10 to destroy the spawned model
            if (Input.GetKeyDown(KeyCode.F10) && modelSpawned)
            {
                DestroyModel();
            }
        }

        private void LoadAndSpawnModel()
        {
            try
            {
                MelonLogger.Msg("Attempting to load asset bundle...");

                if (!File.Exists(assetBundlePath))
                {
                    MelonLogger.Error($"Asset bundle not found at path: {assetBundlePath}");
                    return;
                }

                // Load the asset bundle
                loadedBundle = Il2CppAssetBundleManager.LoadFromFile(assetBundlePath);
                if (loadedBundle == null)
                {
                    MelonLogger.Error("Failed to load asset bundle!");
                    return;
                }

                MelonLogger.Msg("Asset bundle loaded successfully!");

                // Load the model from the bundle
                GameObject modelPrefab = loadedBundle.LoadAsset<GameObject>(modelName);
                if (modelPrefab == null)
                {
                    MelonLogger.Error($"Model '{modelName}' not found in asset bundle!");
                    loadedBundle.Unload(false);
                    return;
                }

                MelonLogger.Msg($"Model '{modelName}' loaded successfully!");

                // Instantiate the model in front of the camera
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 3f;
                    spawnedModel = GameObject.Instantiate(modelPrefab, spawnPosition, Quaternion.identity);

                    // Make the model face the camera
                    spawnedModel.transform.LookAt(2 * spawnedModel.transform.position - mainCamera.transform.position);

                    MelonLogger.Msg("Model spawned successfully!");
                    modelSpawned = true;
                }
                else
                {
                    MelonLogger.Error("Main camera not found!");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error loading/spawning model: {ex.Message}");
                MelonLogger.Error(ex.StackTrace);
            }
        }

        private void DestroyModel()
        {
            try
            {
                if (spawnedModel != null)
                {
                    GameObject.Destroy(spawnedModel);
                    spawnedModel = null;
                    MelonLogger.Msg("Model destroyed!");
                }

                if (loadedBundle != null)
                {
                    loadedBundle.Unload(true);
                    loadedBundle = null;
                    MelonLogger.Msg("Asset bundle unloaded!");
                }

                modelSpawned = false;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error destroying model: {ex.Message}");
            }
        }

        public override void OnApplicationQuit()
        {
            // Clean up on application exit
            DestroyModel();
        }
    }
}