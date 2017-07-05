using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace AssetBundleBuilder
{
    public class SelectBuilderWindow : EditorWindow
    {

        [MenuItem(ABBUtility.Menu_SelectBuildWindow)]
        static void BuildSingleAssetBundle()
        {
            EditorWindow.GetWindow<SelectBuilderWindow>("局部AssetBundle", true);
        }

        public string assetBundleName;
        public string buildpath = "";
        public BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.None;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public bool genFile = false;
        private SerializedProperty script;

        private Object[] selectionGameObject;
        private Object[] selectionMaterial;
        private Object[] selectionTexture;
        private const string Perfer_buildPath = "selectbuildPath";

        void OnEnable()
        {
            script = new SerializedObject(this).FindProperty("m_Script");
            SelectionChanged();
            Selection.selectionChanged += SelectionChanged;
            if (EditorPrefs.HasKey(Perfer_buildPath))
            {
                buildpath = EditorPrefs.GetString(Perfer_buildPath);
            }
        }


        void OnGUI()
        {
            EditorGUILayout.PropertyField(script);
            EditorGUILayout.BeginHorizontal();
            buildpath = EditorGUILayout.TextField("ExportTo", buildpath);
            if (GUILayout.Button("选择路径"))
            {
                var path = EditorUtility.SaveFolderPanel("选择保存路径", buildpath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    buildpath = path;
                    EditorPrefs.SetString(Perfer_buildPath, buildpath);
                    this.Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("BuildTarget", buildTarget);

            assetBundleName = EditorGUILayout.TextField("AbName", assetBundleName);

            if (GUILayout.Button("BuildGameObjects"))
            {
                ABBUtility.BuildSelectAssets(assetBundleName, buildpath, selectionGameObject, buildTarget);
            }
            else if (GUILayout.Button("BuildMaterials"))
            {
                ABBUtility.BuildSelectAssets(assetBundleName, buildpath, selectionMaterial, buildTarget);
            }
            else if (GUILayout.Button("BuildTextures"))
            {
                ABBUtility.BuildSelectAssets(assetBundleName, buildpath, selectionTexture, buildTarget);
            }
        }
        void SelectionChanged()
        {
            selectionGameObject = Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets);
            selectionMaterial = Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets);
            selectionTexture = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);

            if (Selection.activeObject != null)
            {
                assetBundleName = Selection.activeObject.name;
            }
        }
    }
}
