using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.IO;
public static class ABBUtility  {
    public const string Menu_BuildWindow = "AssetBundle/AssetBundleBuilder/BuilderWindow";
    public const string Menu_BundleName = "AssetBundle/AssetBundleBuilder/BundleView";
    public const string Menu_QuickSetter = "AssetBundle/AssetBundleBuilder/QuickSetter";
    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    public static void RecursiveSub(string path, string ignoreFileExt = ".meta", string ignorFolderEnd = "_files", Action<string> action = null)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(ignoreFileExt)) continue;
            action(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            if (dir.EndsWith(ignorFolderEnd)) continue;
            RecursiveSub(dir, ignoreFileExt, ignorFolderEnd, action);
        }
    }
    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    public static void Recursive(string path, string fileExt, bool deep = true, Action<string> action = null)
    {
        string[] names = Directory.GetFiles(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.ToLower().Contains(fileExt.ToLower()))
                action(filename.Replace('\\', '/'));
        }
        if (deep)
        {
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                Recursive(dir, fileExt, deep, action);
            }
        }

    }
}
