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
    public BaseObj thisObj;

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
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        EP += Time.deltaTime * 1f;
        if(EP > MaxEP) EP = MaxEP;
    }
    public abstract void OnApply(int index);
    public abstract void OnDestroyThis();
}
