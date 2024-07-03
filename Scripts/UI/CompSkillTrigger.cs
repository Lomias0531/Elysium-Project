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

    // Start is called before the first frame update
    void Start()
    {
        btn_Click.onClick.AddListener(ApplySkill);
    }

    // Update is called once per frame
    void Update()
    {
        
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
        selectMenu.ShowDescription(thisComp.functions[skillIndex].functionDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        selectMenu.ShowDescription("");
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
        }
    }
}
