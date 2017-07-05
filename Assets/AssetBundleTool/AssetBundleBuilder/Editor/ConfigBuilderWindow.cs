using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace AssetBundleBuilder
{
    public class ConfigBuilderWindow : EditorWindow
    {
        [MenuItem(ABBUtility.Menu_ConfigBuildWindow)]
        static void BuildSingleAssetBundle()
        {
            EditorWindow.GetWindow<ConfigBuilderWindow>("局部AssetBundle", true);
        }

    }

}
