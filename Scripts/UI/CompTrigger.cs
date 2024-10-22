using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompTrigger : BaseCompTrigger
{
    BaseComponent thisComp;
    UnitSelectMenu thisMenu;

    public Image img_Mask;
    public Image img_PowerOff;
    public Button btn_Trigger;
    // Start is called before the first frame update
    void Start()
    {
        btn_Trigger.onClick.AddListener(TriggerThis);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCompStatus();
    }
    public void InitThis(BaseComponent _comp, UnitSelectMenu _menu)
    {
        thisComp = _comp;
        thisMenu = _menu;
        img_Icon.sprite = Tools.GetIcon(thisComp.thisCompData.ComponentIconPath, thisComp.thisCompData.ComponentIconIndex);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        //base.OnPointerEnter(eventData);
        UIController.Instance.DisplayHoveredCompInfo(thisComp);
        thisMenu.HoveringComponent(thisComp);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        //base.OnPointerExit(eventData);
        UIController.Instance.HideHoveredCompInfo();
        thisMenu.HoveringComponent(null);
    }
    void TriggerThis()
    {
        StartCoroutine(thisMenu.ShowCompFunction(thisComp));
    }
    void UpdateCompStatus()
    {
        if (thisComp.functionTimeRequired == 0)
        {
            img_Mask.fillAmount = 0;
        }else
        {
            img_Mask.fillAmount = thisComp.functionTimeElapsed / thisComp.functionTimeRequired;
        }
    }
}
