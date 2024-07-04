using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompMobile : BaseComponent
{
    public override void OnApply(int index)
    {
        var moveType = (BaseObj.MoveType)functions[index].functionIntVal[0];
        var moveStyle = (BaseObj.MoveStyle)functions[index].functionIntVal[1];
        var mobility = (int)functions[index].functionValue;

        PlayerController.Instance.GetMoveRange(Tools.GetMobileRange(thisObj, moveType, moveStyle, mobility));
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
