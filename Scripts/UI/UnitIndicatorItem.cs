using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitIndicatorItem : MonoBehaviour
{
    public BaseObj obj_TargetedItem;

    public Text txt_Name;
    public Image img_Frame;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LocateAndScaleThis();
    }
    public void InitThis(BaseObj obj)
    {
        obj_TargetedItem = obj;
    }
    void LocateAndScaleThis()
    {
        txt_Name.text = obj_TargetedItem.objName;
        if(obj_TargetedItem.Faction == "Elysium")
        {
            img_Frame.color = Color.green;
        }else
        {
            img_Frame.color = Color.red;
        }

        var pos = Camera.main.WorldToScreenPoint(obj_TargetedItem.gameObject.transform.position);
        this.transform.position = pos;
    }
}
