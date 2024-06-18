using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : Singletion<PlayerDataManager>
{
    public float OrganicAmount;
    public float OrganicMaxAmount;
    public float ConstructMaterialAmount;
    public float ConstructMaterialMaxAmount;
    public float MetalAmount;
    public float MetalMaxAmount;
    public float EnergyProduced;
    public float EnergyConsumed;
}
