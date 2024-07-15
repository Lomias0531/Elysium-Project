using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseConstruction : BaseObj
{
    public float EnergyProduced
    {
        get
        {
            float val = 0;
            var generator = this.GetDesiredComponent<CompGenerator>();
            if(generator != null )
            {
                val += generator.powerCapacity;
            }
            return val;
        }
    }
    public float EnergyConsumed;

    public float BuildProgress;
    public override void OnBeingDestroyed()
    {
        
    }

    public override void OnInteracted()
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

    public override void OnSelected()
    {
        PlayerController.Instance.GetPowerGridRange();
    }

    public override void OnUnselected()
    {
        
    }
}
