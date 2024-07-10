using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitIndicatorItem : MonoBehaviour
{
    public BaseObj obj_TargetedItem;

    public Text txt_Name;
    public Image img_Frame;
    public ProceduralImage bg;
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

        var dir = obj_TargetedItem.gameObject.transform.position - Camera.main.transform.position;
        var tes = Vector3.Dot(CameraController.Instance.obj_CameraFocusDummy.transform.forward, dir);
        if(tes >= 0)
        {
            txt_Name.enabled = false;
            img_Frame.enabled = false;
            bg.enabled = false;
        }else
        {
            txt_Name.enabled = true;
            img_Frame.enabled = true;
            bg.enabled = true;
        }

        var pos = Camera.main.WorldToScreenPoint(obj_TargetedItem.gameObject.transform.position);
        this.transform.position = pos;
    }
}
