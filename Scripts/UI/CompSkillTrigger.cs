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
        if (thisComp.functions[skillIndex].functionIconPath != null)
            img_Icon.sprite = Tools.GetIcon(thisComp.functions[skillIndex].functionIconPath, thisComp.functions[skillIndex].functionIconIndex);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        UIController.Instance.DisplayHoveredSkillInfo(thisComp.functions[skillIndex], UIController.DisplayInfoType.skill);
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
    }
    void ApplySkill()
    {
        if (isAvailable)
        {
            thisComp.thisObj.curSelectedComp = thisComp;
            thisComp.thisObj.curSelectedFunction = thisComp.functions[skillIndex];
            thisComp.OnApply(skillIndex);

            //PlayerController.Instance.EntityFinishedAction();
        }
    }
    void UpdateAvailable()
    {
        img_FunctionMask.fillAmount = thisComp.functionTimeElapsed / thisComp.thisObj.curSelectedFunction.functionApplyTimeInterval;
        if (thisComp.thisObj.curSelectedFunction.functionApplyTimeInterval == 0) img_FunctionMask.fillAmount = 0;

        bool isPowerSufficent = true;
        var thisBase = thisComp.thisObj.GetDesiredComponent<CompBase>();
        if (thisBase != null)
        {
            isPowerSufficent = thisBase.isPowerSufficent;
        }

        if (thisComp.EP < thisComp.functions[skillIndex].functionConsume || thisComp.HP <= 0 || !thisComp.isAvailable || !isPowerSufficent)
        {
            isAvailable = false;
            btn_Click.interactable = false;
            img_PowerOff.gameObject.SetActive(true);
        }else
        {
            isAvailable = true;
            btn_Click.interactable = true;
            img_PowerOff.gameObject.SetActive(false);
        }
    }
}
