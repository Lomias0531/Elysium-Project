using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompBase : BaseComponent
{
    public Image img_PowerOff;
    public bool isPowerSufficent
    {
        get
        {
            return PlayerDataManager.Instance.EnergyConsumed <= PlayerDataManager.Instance.EnergyProduced;
        }
    }
    public override void OnApply(int index)
    {
        
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
        if(isPowerSufficent)
        {
            img_PowerOff.gameObject.SetActive(false);
        }else
        {
            img_PowerOff.gameObject.SetActive(true);
        }
    }
}
