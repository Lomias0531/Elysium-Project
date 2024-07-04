using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompSkillTrigger : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    BaseComponent thisComp;
    bool isAvailable = true;

    public Image img_Icon;
    UnitSelectMenu selectMenu;
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
    public void InitThis(bool _isAvailable, UnitSelectMenu menu, int _skillIndex, BaseComponent comp)
    {
        isAvailable = _isAvailable;
        selectMenu = menu;
        thisComp = comp;
        
        skillIndex = _skillIndex;
        if(thisComp.functions[skillIndex].functionIcon != null)
            img_Icon.sprite = thisComp.functions[skillIndex].functionIcon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIController.Instance.DisplayHoveredSkillInfo(thisComp.functions[skillIndex]);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIController.Instance.HideHoveredSkillInfo();
    }
    public void OnPointerClick(PointerEventData eventData)
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

        if (thisComp.EP < thisComp.functions[skillIndex].functionConsume || thisComp.HP <= 0 || !thisComp.isAvailable)
        {
            isAvailable = false;
            btn_Click.interactable = false;
        }else
        {
            isAvailable = true;
            btn_Click.interactable = true;
        }
    }
}
