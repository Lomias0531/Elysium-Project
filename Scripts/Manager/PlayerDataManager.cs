using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : Singletion<PlayerDataManager>
{
    public float OrganicAmount;
    public float OrganicMaxAmount
    {
        get
        {
            float value = 0;
            foreach (var construct in myConstructions)
            {
                value += construct.OrganicStorage;
            }
            return value;
        }
    }
    public float ConstructMaterialAmount;
    public float ConstructMaterialMaxAmount
    {
        get
        {
            float value = 0;
            foreach (var construct in myConstructions)
            {
                value += construct.ConstructStorage;
            }
            return value;
        }
    }
    public float MetalAmount;
    public float MetalMaxAmount
    {
        get
        {
            float value = 0;
            foreach (var construct in myConstructions)
            {
                value += construct.MetalStorage;
            }
            return value;
        }
    }
    public float EnergyProduced
    {
        get
        {
            float value = 0;
            foreach (var construct in myConstructions)
            {
                value += construct.EnergyProduced;
            }
            foreach (var unit in myUnits)
            {
                value += unit.EPMax;
            }
            return value;
        }
    }
    public float EnergyConsumed
    {
        get
        {
            float value = 0;
            foreach (var construct in myConstructions)
            {
                value += construct.EnergyConsumed;
            }
            foreach (var unit in myUnits)
            {
                value += unit.EP;
            }
            return value;
        }
    }

    public List<BaseConstruction> myConstructions = new List<BaseConstruction>();
    public List<BaseUnit> myUnits = new List<BaseUnit>();
}
