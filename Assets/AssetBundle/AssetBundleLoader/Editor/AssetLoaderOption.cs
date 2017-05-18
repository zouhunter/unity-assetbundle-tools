using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class AssetLoaderOption : Editor {

        [MenuItem("Assets/AssetBundleLoader/Simulation")]
        static void SetSimulation()
        {
            AssetBundleLoader.SimulateAssetBundleInEditor = !AssetBundleLoader.SimulateAssetBundleInEditor;
        }
        [MenuItem("Assets/AssetBundleLoader/Simulation", true)]
        static bool SetSimuLationEnable()
        {
            Menu.SetChecked("Assets/AssetBundleLoader/Simulation", AssetBundleLoader.SimulateAssetBundleInEditor);
            return true;
        }

    }
