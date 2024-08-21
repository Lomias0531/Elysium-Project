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
    public float Defense;
    public CompFunctionDetail[] functions;
    [HideInInspector]
    public BaseObj thisObj;
    public string compResID;
    public bool isCritical;

    public float functionTimeElapsed = 0;
    public bool isAvailable = true;

    public enum InteractFunction
    {
        Harvest,
        Active,
        Store,
        Construct,
    }
    // Start is called before the first frame update
    public virtual void Start()
    {
        LoadCompDataViaID();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        EP += Time.deltaTime * 0.1f;
        if(EP > MaxEP) EP = MaxEP;

        functionTimeElapsed -= Time.deltaTime;
        if(functionTimeElapsed <= 0) functionTimeElapsed = 0;

        isAvailable = functionTimeElapsed <= 0;
    }
    public abstract void OnApply(int index);
    public abstract void OnDestroyThis();
    public abstract void OnTriggerFunction(params object[] obj);
    public void FunctionTriggered(CompFunctionDetail function)
    {
        thisObj.curSelectedComp = this;
        thisObj.curSelectedFunction = function;
        functionTimeElapsed = function.functionApplyTimeInterval;
        EP -= function.functionConsume;
    }
    public void LoadCompDataViaID()
    {
        if (!string.IsNullOrEmpty(compResID))
        {
            //SO_ComponentData compData = DataController.Instance.GetComponentData(compResID);
            var compData = DataController.Instance.GetComponentData(compResID);

            compName = compData.ComponentName;
            HP = compData.ComponentEndurance;
            MaxHP = compData.ComponentEndurance;
            EP = compData.ComponentInternalBattery;
            MaxEP = compData.ComponentInternalBattery;
            isCritical = compData.isFatalComponent;
            functions = compData.functions;
            Defense = compData.ComponentDefense;
            isCritical = compData.isFatalComponent;
        }
    }
}
