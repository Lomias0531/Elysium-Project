using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnitSpot : MonoBehaviour
{
    public ComponentBaseType attachType;
    public int spotKey;
    public bool isOccupied = false;
}
public enum ComponentAttachType
{
    Ordinary,
    Agressive,
    Funcional,
    Protective,
}
public enum ComponentBaseType
{
    Universal,
    Ordinary,
    Agressive,
    Funcional,
    Protective,
}