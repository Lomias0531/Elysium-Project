using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompSkillTrigger : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    BaseComponent thisComp;
    bool isAvailable = true;

    public Image img_Icon;
    UnitSelectMenu selectMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitThis(bool _isAvailable, UnitSelectMenu menu, CompFunctionDetail function, BaseComponent comp)
    {
        isAvailable = _isAvailable;
        selectMenu = menu;
        thisComp = comp;
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAvailable)
        {
            thisComp.OnApply();
        }
    }
}
