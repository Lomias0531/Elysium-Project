using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompHarvest : BaseComponent
{
    public override void OnApply(int index)
    {
        PlayerController.Instance.GetInteractRange(InteractFunction.Harvest);
    }

    public override void OnDestroyThis()
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
