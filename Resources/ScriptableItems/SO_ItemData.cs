using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Data/Item", order = 1)]
[PreferBinarySerialization]
public class SO_ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public int maxStackCount;
    public float itemWeight;
    public bool isConsumeable;
    public Sprite itemIcon;
}
