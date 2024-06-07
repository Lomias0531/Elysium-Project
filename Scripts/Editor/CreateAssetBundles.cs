using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    /*MenuItem属性允许你添加菜单项到主菜单和检视面板上下文菜单。 
    该属性把任意静态函数变为一个菜单命令。仅静态函数能使用这个MenuItem属性*/
    [MenuItem("Assets/Build AssetBundles")]

    static void BulidAllAssetBundles()
    {
        string dir = "AssetBundles";
        if (Directory.Exists(dir) == false)
            Directory.CreateDirectory(dir);
        //输出路径 打包压缩格式 打包到的平台
        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
    }
}