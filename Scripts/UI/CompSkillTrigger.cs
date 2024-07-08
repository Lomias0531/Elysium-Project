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
    public void InitThis(bool _isAvailable, int _skillIndex, BaseComponent comp)
    {
        isAvailable = _isAvailable;
        thisComp = comp;
        
        skillIndex = _skillIndex;
        if(thisComp.functions[skillIndex].functionIcon != null)
            img_Icon.sprite = thisComp.functions[skillIndex].functionIcon;

        img_BG.material.SetTexture("_Sprite", img_BG.mainTexture);
        img_Icon.material.SetTexture("_Sprite", img_Icon.mainTexture);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        UIController.Instance.DisplayHoveredSkillInfo(thisComp.functions[skillIndex], UIController.DisplayInfoType.skill);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        UIController.Instance.HideHoveredSkillInfo();
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        ApplySkill();
    }
    void ApplySkill()
    {
        if (isAvailable)
        {
            thisComp.OnApply(skillIndex);
            thisComp.thisObj.curSelectedComp = thisComp;
            thisComp.thisObj.curSelectedFunction = thisComp.functions[skillIndex];
        }
    }
    void UpdateAvailable()
    {
        img_FunctionMask.fillAmount = thisComp.functionTimeElapsed / thisComp.thisObj.curSelectedFunction.functionApplyTimeInterval;
        if (thisComp.thisObj.curSelectedFunction.functionApplyTimeInterval == 0) img_FunctionMask.fillAmount = 0;

        if (thisComp.EP < thisComp.functions[skillIndex].functionConsume || thisComp.HP <= 0 || !thisComp.isAvailable)
        {
            isAvailable = false;
            btn_Click.interactable = false;
            //img_Icon.color = Color.black;
        }else
        {
            isAvailable = true;
            btn_Click.interactable = true;
            //img_Icon.color = Tools.HexToColor("#007DFF");
        }
    }
}
