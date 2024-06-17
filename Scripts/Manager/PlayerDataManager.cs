using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : Singletion<PlayerDataManager>
{
    public float OrganicAmount;
    public float ConstructMaterialAmount;
    public float MetalAmount;

    public float EnergyProduced;
    public float EnergyConsumed;
}
