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
    public ComponentFunction[] functions;
    public float ApplyTimeInterval;

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
    void Update()
    {
        
    }
    public abstract void OnApply();
    public abstract void OnDestroyThis();
}
