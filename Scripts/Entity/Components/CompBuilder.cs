using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompBuilder : BaseComponent
{
    public int buildRange;
    public override void OnApply(int index)
    {
        PlayerController.Instance.GetBuildRange();
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
    }
}
