using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Entity", menuName = "Data/Entity", order = 2)]
[PreferBinarySerialization]
public class SO_EntityData : ScriptableObject
{
    public string EntityID;
    public GameObject EntityObject;
}
