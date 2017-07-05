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
        public class LayerNode
        {
            public bool isExpanded { get; private set; }
            public bool isFolder;
            public bool selected { get; private set; }
            public int indent = 0;
            public GUIContent content;
            public LayerNode parent;
            public List<LayerNode> childs = new List<LayerNode>();
            public Object layer;
            private GUIContent _groupff;
            private GUIContent _groupOn;
            private GUIContent _spritenormal;
            public LayerNode(string path)
            {
                this.layer = AssetDatabase.LoadAssetAtPath(path,typeof(Object));
                isFolder = ProjectWindowUtil.IsFolder(layer.GetInstanceID());

                if (isFolder)
                {
                    _groupff = new GUIContent(layer.name, EditorGUIUtility.IconContent("IN foldout focus").image);
                    _groupOn = new GUIContent(layer.name, EditorGUIUtility.IconContent("IN foldout focus on").image);
                }
                else
                {
                    _spritenormal = new GUIContent(layer.name, EditorGUIUtility.IconContent("createrect").image);
                }

                content = isFolder ? _groupOn : _spritenormal;
                isExpanded = true;
            }
            public LayerNode(Object layer)
            {
                this.layer = layer;

                isFolder = ProjectWindowUtil.IsFolder(layer.GetInstanceID());
       
                if (isFolder)
                {
                    _groupff = new GUIContent(layer.name, EditorGUIUtility.IconContent("IN foldout focus").image);
                    _groupOn = new GUIContent(layer.name, EditorGUIUtility.IconContent("IN foldout focus on").image);
                }
                else
                {
                    _spritenormal = new GUIContent(layer.name, EditorGUIUtility.IconContent("createrect").image);
                }

                content = isFolder ? _groupOn : _spritenormal;
                isExpanded = true;
            }

            public void Expland(bool on)
            {
                content = on ? _groupOn : _groupff;
                content.text = layer.name;
                isExpanded = on;
            }
            public void Select(bool on)
            {
                selected = on;
                if (childs != null)
                    foreach (var child in childs)
                    {
                        child.Select(on);
                    }
            }
        }
        private SerializedProperty script;
        private ConfigBuildObj buildObj;
        private LayerNode rootNode;
        private const string lastItem = "lastbuildObj";
        private void OnEnable()
        {
            script = new SerializedObject(this).FindProperty("m_Script");
            if (EditorPrefs.HasKey(lastItem))
            {
                var obj = EditorUtility.InstanceIDToObject(EditorPrefs.GetInt(lastItem));
                if(obj != null)
                {
                    buildObj = obj as ConfigBuildObj;
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.PropertyField(script);
            
            if (DrawObjectHolder())
            {
                if (rootNode == null)
                {
                    rootNode = LoadNodesFromObj(buildObj);
                }
                else
                {
                    DrawData(rootNode);
                }
            }
        }
        private bool DrawObjectHolder()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                var obj = EditorGUILayout.ObjectField(buildObj, typeof(ConfigBuildObj), false) as ConfigBuildObj;
                if (obj != buildObj)
                {
                    buildObj = obj;
                    EditorPrefs.SetInt(lastItem, buildObj.GetInstanceID());
                }
                if (GUILayout.Button("创建"))
                {
                    buildObj = ScriptableObject.CreateInstance<ConfigBuildObj>();
                    ProjectWindowUtil.CreateAsset(buildObj, "configBuildObj.asset");
                }
            }
          
            if (buildObj == null)
            {
                DrawErrBox("请先将配制对象放入");
                return false;
            }
            return true;
        }

        private void DrawErrBox(string str)
        {
            EditorGUILayout.HelpBox(str, MessageType.Error);
        }
        private static LayerNode LoadNodesFromObj(ConfigBuildObj buildObj)
        {
            if (buildObj.needBuilds == null)
            {
                return null;
            }
            else
            {
                Dictionary<string, LayerNode> nodeDic = new Dictionary<string, AssetBundleBuilder.ConfigBuilderWindow.LayerNode>();
                foreach (var item in buildObj.needBuilds)
                {
                    var folder = item.assetPath.Remove(item.assetPath.LastIndexOf("/"));
                    if (folder != item.assetPath && !nodeDic.ContainsKey(folder))
                    {
                        nodeDic.Add(folder, new LayerNode(folder));
                        nodeDic[folder].indent = folder.Split('/').Length;
                    }
                    if (!nodeDic.ContainsKey(item.assetPath))
                    {
                        nodeDic.Add(item.assetPath, new LayerNode(item.obj));
                        nodeDic[item.assetPath].indent = item.assetPath.Split('/').Length;
                    }
                }
                LayerNode root = null;
                foreach (var item in nodeDic)
                {
                    foreach (var child in nodeDic)
                    {
                        if (child.Key.Contains(item.Key + "/"))
                        {
                            item.Value.childs.Add(child.Value);
                            child.Value.parent = item.Value;
                            if (root == null || AssetDatabase.GetAssetPath(root.layer).Contains(item.Key))
                            {
                                root = item.Value;
                            }
                        }
                    }
                }
                return root;
            }
           
        }

        private void DrawData(LayerNode data)
        {
            if (data.content != null)
            {
                EditorGUI.indentLevel = data.indent;
                DrawGUIData(data);
            }
            if (data.isExpanded)
                for (int i = 0; i < data.childs.Count; i++)
                {
                    LayerNode child = data.childs[i];
                    if (child.content != null)
                    {
                        EditorGUI.indentLevel = child.indent;
                        if (child.childs.Count > 0)
                        {
                            DrawData(child);
                        }
                        else
                        {
                            DrawGUIData(child);
                        }
                    }
                }
        }
        private void DrawGUIData(LayerNode data)
        {
            GUIStyle style = "Label";
            Rect rt = GUILayoutUtility.GetRect(data.content, style);

            var offset = (16 * EditorGUI.indentLevel);
            var pointWidth = 10;

            var expanded = EditorGUI.Toggle(new Rect(rt.x + offset, rt.y, pointWidth, rt.height), data.isExpanded, style);
            if (data.isExpanded != expanded && data.isFolder)
            {
                data.Expland(expanded);
            }

            var srect = new Rect(rt.x + offset, rt.y, rt.width - offset - pointWidth, rt.height);
            var selected = EditorGUI.Toggle(srect, data.selected, style);
            if (selected != data.selected)
            {
                data.Select(selected);
            }
            if (data.selected)
            {
                EditorGUI.DrawRect(srect, Color.gray);
            }

            EditorGUI.LabelField(rt, data.content);
        }
    }

}
