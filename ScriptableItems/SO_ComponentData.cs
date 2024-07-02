using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Component",menuName ="Data/Component",order =0)]
[PreferBinarySerialization]
public class SO_ComponentData : ScriptableObject
{
    public string ComponentID;
    public string ComponentName;
    public float ComponentEndurance;
    public float ComponentInternalBattery;
    public bool isFatalComponent;
    public CompFunctionDetail[] functions;
}
[Serializable]
public struct CompFunctionDetail
{
    public string functionName;
    public BaseComponent.ComponentFunction function;
    public Sprite functionIcon;
    public float functionApplyTimeInterval;
    public float functionValue;
    public float functionConsume;
    public int[] functionIntVal;
    public float[] functionFloatVal;
    public bool[] functionBoolVal;
    public string[] functionStringVal;
    public string functionDescription;
}