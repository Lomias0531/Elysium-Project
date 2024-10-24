using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompSkillTrigger : BaseCompTrigger
{
    BaseComponent thisComp;
    bool isAvailable = true;

    int skillIndex;
    public Button btn_Click;
    public Image img_FunctionMask;
    public Image img_PowerOff;
    public Image img_Auto;

    UnitSelectMenu menu;

    // Start is called before the first frame update
    void Start()
    {
        btn_Click.onClick.AddListener(ApplySkill);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAvailable();
    }
    public void InitThis(bool _isAvailable, int _skillIndex, BaseComponent comp, UnitSelectMenu _menu)
    {
        isAvailable = _isAvailable;
        thisComp = comp;
        menu = _menu;
        
        skillIndex = _skillIndex;
        if (thisComp.thisCompData.functions[skillIndex].functionIconPath != null)
            img_Icon.sprite = Tools.GetIcon(thisComp.thisCompData.functions[skillIndex].functionIconPath, thisComp.thisCompData.functions[skillIndex].functionIconIndex);

        img_Auto.gameObject.SetActive(thisComp.thisCompData.functions[skillIndex].isAuto);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        UIController.Instance.DisplayHoveredSkillInfo(thisComp.thisCompData.functions[skillIndex], UIController.DisplayInfoType.skill, thisComp.thisCompData.ComponentName);
        menu.HoveringComponent(thisComp);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        UIController.Instance.HideHoveredSkillInfo();
        menu.HoveringComponent(null);
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        //ApplySkill();
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            if(thisComp.thisCompData.functions[skillIndex].canBeAuto)
            {
                thisComp.thisCompData.functions[skillIndex].isAuto = !thisComp.thisCompData.functions[skillIndex].isAuto;
                img_Auto.gameObject.SetActive(thisComp.thisCompData.functions[skillIndex].isAuto);
            }
        }
    }
    void ApplySkill()
    {
        if (isAvailable)
        {
            thisComp.thisObj.curSelectedComp = thisComp;
            thisComp.thisObj.curSelectedFunction = thisComp.thisCompData.functions[skillIndex];
            thisComp.OnApply(skillIndex);

            //PlayerController.Instance.EntityFinishedAction();
        }
    }
    void UpdateAvailable()
    {
        if (thisComp.thisObj.curSelectedComp == null) return;
        img_FunctionMask.fillAmount = thisComp.functionTimeElapsed / thisComp.thisObj.curSelectedFunction.functionApplyTimeInterval;
        if (thisComp.thisObj.curSelectedFunction.functionApplyTimeInterval == 0) img_FunctionMask.fillAmount = 0;

        bool isPowerSufficent = true;
        var thisBase = thisComp.thisObj.GetDesiredComponent<CompBase>();
        if (thisBase != null)
        {
            isPowerSufficent = thisBase.isPowerSufficent;
        }

        if (thisComp.HP <= 0 || !thisComp.isAvailable)
        {
            isAvailable = false;
        }else
        {
            isAvailable = true;
        }

        if(thisComp.EP < thisComp.thisCompData.functions[skillIndex].functionConsume || !isPowerSufficent)
        {
            img_PowerOff.gameObject.SetActive(true);
            isAvailable = false;
        }else
        {
            img_PowerOff.gameObject.SetActive(false);
        }

        btn_Click.interactable = isAvailable;
    }
}
