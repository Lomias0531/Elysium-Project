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
    public ComponentFunctionType componentType;
    public CompFunctionDetail[] functions;
}
