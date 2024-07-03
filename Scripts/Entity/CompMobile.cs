using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompMobile : BaseComponent
{
    //public BaseUnit.MoveStyle moveStyle;
    //public BaseUnit.MoveType moveType;
    //public int mobility;

    public override void OnApply(int index)
    {
        var moveType = (BaseUnit.MoveType)functions[index].functionIntVal[0];
        var moveStyle = (BaseUnit.MoveStyle)functions[index].functionIntVal[1];
        var mobility = (int)functions[index].functionValue;

        PlayerController.Instance.GetMoveRange(Tools.GetMobileRange(thisObj, moveType, mobility));
    }

    public override void OnDestroyThis()
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
