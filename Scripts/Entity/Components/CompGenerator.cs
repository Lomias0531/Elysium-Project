using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompGenerator : BaseComponent
{
    public int powerCapacity;
    public int powerRegenRate;
    public override void OnApply(int index)
    {
        
    }

    public override void OnDestroyThis()
    {
        
    }

    public override void OnTriggerFunction(params object[] obj)
    {
        
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (thisObj.GetDesiredComponent<CompConstructTemp>() != null) return;
        foreach (var comp in thisObj.components)
        {
            comp.EP += powerRegenRate * Time.deltaTime;
            if(comp.EP > comp.MaxEP) comp.EP = comp.MaxEP;
        }
    }
}
