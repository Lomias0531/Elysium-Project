using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseComponent : MonoBehaviour
{
    public float HP;
    public float MaxHP;
    public float EP;
    public float MaxEP;

    public float functionTimeElapsed = 0;
    public bool isAvailable = true;

    public ComponentData thisCompData;
    public BaseObj thisObj;
    public Transform tsf_InstalledSlot;
    // Start is called before the first frame update
    public virtual void Start()
    {
        
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
    public virtual void OnTriggerFunction(ComponentFunctionType type, params object[] obj)
    {

    }
    public void FunctionTriggered(CompFunctionDetail function)
    {
        thisObj.curSelectedComp = this;
        thisObj.curSelectedFunction = function;
        functionTimeElapsed = function.functionApplyTimeInterval;
        EP -= function.functionConsume;
    }
    public void InitThis()
    {
        MaxHP = thisCompData.ComponentEndurance;
        HP = MaxHP;
        MaxEP = thisCompData.ComponentInternalBattery;
        EP = MaxEP;
    }
}
