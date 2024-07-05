using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompItemTrigger : BaseCompTrigger
{
    public Text txt_itemCount;

    CompStorage inv;
    int index;

    UnitSelectMenu menu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateItemInfo();
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        CompFunctionDetail detail = new CompFunctionDetail();
        var itemInfo = DataController.Instance.GetItemInfo(inv.inventory[index].itemID);
        detail.functionName = itemInfo.itemName;
        detail.functionIcon = itemInfo.itemIcon;

        UIController.Instance.DisplayHoveredSkillInfo(detail,UIController.DisplayInfoType.item);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        UIController.Instance.HideHoveredSkillInfo();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (PlayerController.Instance.selectedObject.curSelectedFunction.functionIntVal.Length < 2)
        {
            var list = PlayerController.Instance.selectedObject.curSelectedFunction.functionIntVal.ToList();
            list.Add(index);
            PlayerController.Instance.selectedObject.curSelectedFunction.functionIntVal = list.ToArray();
        }
        PlayerController.Instance.selectedObject.curSelectedFunction.functionIntVal[1] = index;
        PlayerController.Instance.GetInteractRange(BaseComponent.InteractFunction.Store);
    }
    public void InitThis(CompStorage storage, int _index, UnitSelectMenu _menu)
    {
        inv = storage;
        index = _index;
        menu = _menu;

        var itemInfo = DataController.Instance.GetItemInfo(inv.inventory[index].itemID);
        img_Icon.sprite = itemInfo.itemIcon;
        txt_itemCount.text = inv.inventory[index].stackCount.ToString();
    }
    private void UpdateItemInfo()
    {
        if(index >= inv.inventory.Count)
        {
            menu.RemoveTrigger();
            return;
        }
        if(inv.inventory[index].stackCount <= 0)
        {
            menu.RemoveTrigger();
            return;
        }
        txt_itemCount.text = inv.inventory[index].stackCount.ToString();
    }
}
