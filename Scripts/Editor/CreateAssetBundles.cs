using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    /*MenuItem������������Ӳ˵�����˵��ͼ�����������Ĳ˵��� 
    �����԰����⾲̬������Ϊһ���˵��������̬������ʹ�����MenuItem����*/
    [MenuItem("Assets/Build AssetBundles")]

    static void BulidAllAssetBundles()
    {
        string dir = "AssetBundles";
        if (Directory.Exists(dir) == false)
            Directory.CreateDirectory(dir);
        //���·�� ���ѹ����ʽ �������ƽ̨
        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
    }
}