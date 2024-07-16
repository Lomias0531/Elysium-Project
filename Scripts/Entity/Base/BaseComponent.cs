using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseComponent : MonoBehaviour
{
    public string compName;
    public float HP;
    public float MaxHP;
    public float EP;
    public float MaxEP;
    public CompFunctionDetail[] functions;
    [HideInInspector]
    public BaseObj thisObj;
    public string compResID;
    public bool isCritical;

    public float functionTimeElapsed = 0;
    public bool isAvailable = true;

    public enum ComponentFunction
    {
        Mobile,
        Damage,
        Interact,
        Protection,
    }
    public enum InteractFunction
    {
        Harvest,
        Active,
        Store,
    }
    // Start is called before the first frame update
    public virtual void Start()
    {
        if(!string.IsNullOrEmpty(compResID))
        {
            SO_ComponentData compData = DataController.Instance.GetComponentData(compResID);
            if(compData != null)
            {
                compName = compData.name;
                HP = compData.ComponentEndurance;
                MaxHP = compData.ComponentEndurance;
                EP = compData.ComponentInternalBattery;
                MaxEP = compData.ComponentInternalBattery;
                isCritical = compData.isFatalComponent;
                functions = compData.functions;
            }
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {
        EP += Time.deltaTime * 1f;
        if(EP > MaxEP) EP = MaxEP;

        functionTimeElapsed -= Time.deltaTime;
        if(functionTimeElapsed <= 0) functionTimeElapsed = 0;

        isAvailable = functionTimeElapsed <= 0;
    }
    public abstract void OnApply(int index);
    public abstract void OnDestroyThis();
    public void FunctionTriggered(CompFunctionDetail function)
    {
        functionTimeElapsed = function.functionApplyTimeInterval;
        EP -= function.functionConsume;
    }
}
