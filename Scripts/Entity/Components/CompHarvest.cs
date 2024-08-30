using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompHarvest : BaseComponent
{
    //TODO 采集调整为对范围内所有资源进行采集
    public override void OnApply(int index)
    {
        //PlayerController.Instance.GetInteractRange(InteractFunction.Harvest);
    }

    public override void OnDestroyThis()
    {
        
    }

    //public override void OnTriggerFunction(params object[] obj)
    //{
        
    //}

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
}
