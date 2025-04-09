using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Dialogue;
using Il2CppScheduleOne.Economy;
using System.Runtime.CompilerServices;
using static Il2CppScheduleOne.Dialogue.DialogueController;
using System.Security.Cryptography;
using HarmonyLib;
using Il2CppScheduleOne.Vehicles;
using Il2CppSystem.Reflection;
using Il2CppScheduleOne.Map;

[assembly: MelonInfo(typeof(TunerMod.Core), "TunerMod", "1.0.0", "RedReflex", null)]
[assembly: MelonGame("TVGS", "Schedule I")]
namespace TunerMod
{

    public class Core : MelonMod
    {
        public static string[] spoilerList = { "Spoiler 1", "Spoiler 2", "spoiler 3", "spoiler 4", "spoiler 5", "spoiler 6", "spoiler 7", "spoiler 8", "spoiler 9", "spoiler 10", "spoiler 11", "spoiler 12", "spoiler 13", "spoiler 14", "spoiler 15", "spoiler 16", "spoiler 17", "spoiler 18", "spoiler 19", "spoiler 20" };

        string modFolder = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods", "Assets");

        public static string sedanPath = "Sedan(Clone)/Sedan/sedan/Body";
        public static string coupePath = "Coupe(Clone)/Coupe/coupe/Main";
        public static string currentVehicle = coupePath;
        public static Il2CppAssetBundle loadedSpoilerBundle;

        [HarmonyPatch(typeof(LandVehicle), "Awake")]
        private class Patch
        {
            private static void Prefix(LandVehicle __instance)
            {
                MelonLogger.Msg("PRE Awake method called!");
                MelonLogger.Msg("Current Vehicle: " + __instance.name);
                if(__instance.transform.FindChild("OwnedVehiclePoI").GetComponent<POI>().enabled)
                {
                    GameObject spoilerSocket = CreateSpoilerSocket(__instance.name, __instance.transform.parent);
                    AddSpoilerToVehicle(spoilerSocket);
                }
            }
            
            private static void Postfix(LandVehicle __instance)
            {
                MelonLogger.Msg("POST Awake method called!");
                MelonLogger.Msg("Current Vehicle: " + __instance.name);
            }
        }

        public static void InitializeSpoilerBundle()
        {
            MelonLogger.Msg("Initializing Spoiler Bundle...");
            string spoilerAssetBundlePath = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods", "Assets", "spoilerbundle.asset");
            if (!File.Exists(spoilerAssetBundlePath))
            {
                MelonLogger.Error($"Spoiler asset bundle not found at path: {spoilerAssetBundlePath}");
                return;
            }
            loadedSpoilerBundle = Il2CppAssetBundleManager.LoadFromFile(spoilerAssetBundlePath);
            if (loadedSpoilerBundle == null)
            {
                MelonLogger.Error("Failed to load spoiler asset bundle!");
                return;
            }
            MelonLogger.Msg("Spoiler asset bundle loaded successfully!");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if(sceneName == "Main")
            {
                InitializeSpoilerBundle();
                InitializeNPCChoices();
            }
        }

        public static GameObject CreateSpoilerSocket(string vehicleName, GameObject vehicleRef)
        {

            if(vehicleRef == null)
            {
                MelonLogger.Error("Vehicle reference is null!");
                return null;
            }

            Vector3 position;
            Quaternion rotation;


            if (vehicleName == "Coupe(Clone)")
            {
                position = new Vector3(-3.35f, 0f, 0.35f);
                rotation = Quaternion.Euler(0, 90, 90);
            }
            else if (vehicleName == "Sedan(Clone)")
            {
                position = new Vector3(-0f, 2.2f, 0.425f);
                rotation = Quaternion.Euler(90, 0, 0);
            } else
            {
                MelonLogger.Error("Vehicle reference is not a valid vehicle type!");
                return null;
            }


            GameObject spoilerSocket = new GameObject("SpoilerSocket");
            spoilerSocket.transform.SetParent(vehicleRef.transform);
            spoilerSocket.transform.localPosition = position;
            spoilerSocket.transform.localRotation = rotation;
            spoilerSocket.layer = vehicleRef.layer;

            return spoilerSocket;

        }


        public static void AddSpoilerToVehicle(GameObject socketRef)
        {
            try
            {
                if (socketRef == null)
                {
                    MelonLogger.Error("Spoiler socket reference is null!");
                    return;
                }

                if (loadedSpoilerBundle == null)
                {
                    MelonLogger.Error("Spoiler asset bundle is not loaded!");
                    return;
                }

                if (spoilerList == null)
                {
                    MelonLogger.Error("Spoiler list is null!");
                    return;
                }
                Material VehicleMaterial = GameObject.Find(currentVehicle).GetComponent<MeshRenderer>().material;

                foreach (string spoiler in spoilerList)
                {
                    GameObject spoilerPrefab = loadedSpoilerBundle.LoadAsset<GameObject>(spoiler);
                    if(spoilerPrefab == null)
                    {
                        MelonLogger.Error($"Spoiler prefab '{spoiler}' not found in asset bundle!");
                        continue;
                    }

                    GameObject spawnedSpoiler = GameObject.Instantiate(spoilerPrefab, socketRef.transform.position, socketRef.transform.rotation);
                    if(spawnedSpoiler == null)
                    {
                        MelonLogger.Error($"Failed to instantiate spoiler '{spoiler}'!");
                        continue;
                    }

                    spawnedSpoiler.active = false;
                    spawnedSpoiler.transform.SetParent(socketRef.transform);
                    spawnedSpoiler.transform.localPosition = Vector3.zero;
                    spawnedSpoiler.transform.localRotation = Quaternion.identity;
                    spawnedSpoiler.layer = socketRef.layer;
                    Transform[] spoilerChildren = spawnedSpoiler.GetComponentsInChildren<Transform>(true);
                    if (spoilerChildren == null || spoilerChildren.Length == 0)
                    {
                        MelonLogger.Error($"No children found for spoiler '{spoiler}'!");
                        continue;
                    }

                    foreach (Transform child in spoilerChildren)
                    {
                        if (child != spawnedSpoiler.transform)
                        {
                            Debug.Log("Child found: " + child.gameObject.name);
                            GameObject childGameObject = child.gameObject;
                            MeshRenderer childMeshRenderer = childGameObject.GetComponent<MeshRenderer>();
                            childMeshRenderer.material = VehicleMaterial;
                        }
                    }
                }

                MelonLogger.Msg("Loaded all spoilers");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error adding spoiler to vehicle: {ex.Message}");
                MelonLogger.Error(ex.StackTrace);

            }
        }

        public static void InitializeNPCChoices()
        {
            MelonLogger.Msg("Initializing NPC Choices...");
            GameObject MarcoNPCDialogueObject = GameObject.Find("Marco/Dialogue");
            if (MarcoNPCDialogueObject == null)
            {
                MelonLogger.Error("Marco NPC Dialogue object not found!");
                return;
            }
            DialogueController MarcoNPCDialogueController = MarcoNPCDialogueObject.GetComponent<DialogueController>();
            if (MarcoNPCDialogueController == null)
            {
                MelonLogger.Error("Marco NPC Dialogue Controller not found!");
                return;
            }
            // Add new dialogue choice
            DialogueChoice newChoice = new DialogueChoice();
            newChoice.ChoiceText = "I'd like to upgrade my vehicle";
            newChoice.Enabled = true;
            newChoice.isValidCheck = MarcoNPCDialogueController.Choices[0].isValidCheck;
            newChoice.onChoosen = MarcoNPCDialogueController.Choices[0].onChoosen;
            MarcoNPCDialogueController.AddDialogueChoice(newChoice);
            MelonLogger.Msg("NPC Choices initialized successfully!");
        }

        public override void OnUpdate()
        {

            if (Input.GetKeyDown(KeyCode.F9))
            {
                GameObject vehicle = GameObject.Find(currentVehicle);
                GameObject spoilerSocket = CreateSpoilerSocket("Coupe", vehicle);
                GameObject sedanVehicle = GameObject.Find(currentVehicle);
                GameObject sedanSpoilerSocket = CreateSpoilerSocket("Sedan", sedanVehicle);
                AddSpoilerToVehicle(spoilerSocket);
                AddSpoilerToVehicle(sedanSpoilerSocket);
            }

            //// Press F9 to load and spawn the model
            //if (Input.GetKeyDown(KeyCode.F9) && !modelSpawned)
            //{
            //    LoadAndSpawnModel();
            //}

            //// Press F10 to destroy the spawned model
            //if (Input.GetKeyDown(KeyCode.F10) && modelSpawned)
            //{
            //    DestroyModel();
            //}
        }

        //private void ReplaceVehicleModel()
        //{
        //    try
        //    {
        //        MelonLogger.Msg("Attempting to load asset bundle...");

        //        if (!File.Exists(skylineAssetBundlePath))
        //        {
        //            MelonLogger.Error($"Asset bundle not found at path: {skylineAssetBundlePath}");
        //            return;
        //        }

        //        // Load the asset bundle
        //        Il2CppAssetBundle loadedBundle = Il2CppAssetBundleManager.LoadFromFile(skylineAssetBundlePath);
        //        if (loadedBundle == null)
        //        {
        //            MelonLogger.Error("Failed to load asset bundle!");
        //            return;
        //        }

        //        MelonLogger.Msg("Asset bundle loaded successfully!");

        //        // Load the model from the bundle
        //        GameObject modelPrefab = loadedBundle.LoadAsset<GameObject>(skylineName);
        //        if (modelPrefab == null)
        //        {
        //            MelonLogger.Error($"Model '{skylineName}' not found in asset bundle!");
        //            loadedBundle.Unload(false);
        //            return;
        //        }

        //        MelonLogger.Msg($"Model '{skylineName}' loaded successfully!");


        //        GameObject vehicleToReplace = GameObject.Find("Sedan(Clone)/Sedan/sedan/Body");

        //        if (vehicleToReplace == null)
        //        {
        //            MelonLogger.Error("Vehicle to replace not found!");
        //            return;
        //        }

        //        Vector3 spawnPosition = vehicleToReplace.transform.position + vehicleToReplace.transform.forward;

        //        GameObject spawnedModel = GameObject.Instantiate(modelPrefab, spawnPosition, Quaternion.identity);

        //        // Try get MeshFilter and MeshRenderer
        //        MeshFilter newMeshFilter = spawnedModel.transform.GetChild(0).GetComponent<MeshFilter>();
        //        MeshRenderer newMeshRenderer = spawnedModel.transform.GetChild(0).GetComponent<MeshRenderer>();

        //        MeshFilter targetMeshFilter = vehicleToReplace.GetComponent<MeshFilter>();
        //        MeshRenderer targetMeshRenderer = vehicleToReplace.GetComponent<MeshRenderer>();

        //        if (newMeshFilter == null || newMeshRenderer == null)
        //        {
        //            MelonLogger.Error("Spawned model is missing MeshFilter or MeshRenderer!");
        //            //GameObject.Destroy(spawnedModel);
        //            return;
        //        }

        //        if (targetMeshFilter == null || targetMeshRenderer == null)
        //        {
        //            MelonLogger.Error("Target vehicle is missing MeshFilter or MeshRenderer!");
        //            GameObject.Destroy(spawnedModel);
        //            return;
        //        }

        //        // Copy mesh and material
        //        targetMeshFilter.mesh = newMeshFilter.sharedMesh;
        //        targetMeshRenderer.materials = newMeshRenderer.sharedMaterials;


        //        // Clean up
        //        GameObject.Destroy(spawnedModel);
        //        MelonLogger.Msg("Vehicle body replaced successfully.");

        //    }
        //    catch (System.Exception ex)
        //    {
        //        MelonLogger.Error($"Error loading/spawning model: {ex.Message}");
        //        MelonLogger.Error(ex.StackTrace);
        //    }
        //}

        //private void DestroyModel()
        //{
        //    try
        //    {
        //        if (spawnedModel != null)
        //        {
        //            GameObject.Destroy(spawnedModel);
        //            spawnedModel = null;
        //            MelonLogger.Msg("Model destroyed!");
        //        }

        //        if (loadedBundle != null)
        //        {
        //            loadedBundle.Unload(true);
        //            loadedBundle = null;
        //            MelonLogger.Msg("Asset bundle unloaded!");
        //        }

        //        modelSpawned = false;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        MelonLogger.Error($"Error destroying model: {ex.Message}");
        //    }
        //}

        //public override void OnApplicationQuit()
        //{
        //    // Clean up on application exit
        //    DestroyModel();
        //}
    }
}