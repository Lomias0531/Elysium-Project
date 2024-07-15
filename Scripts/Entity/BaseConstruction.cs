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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
