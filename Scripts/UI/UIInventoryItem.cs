using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventoryItem : MonoBehaviour
{
    public Image img_Item;
    public Text txt_StackCount;

    public void UpdateInventory(ItemData item)
    {
        if(string.IsNullOrEmpty(item.itemID))
        {
            //img_Item.enabled = false;
            //txt_StackCount.enabled = false;
            return;
        }
        img_Item.enabled = true;
        txt_StackCount.enabled = true;
        var id = item.itemID;
        var data = DataController.Instance.GetItemData(id);
        img_Item.sprite = Tools.GetIcon(data.itemIconPath, data.itemIconIndex);
        txt_StackCount.text = item.stackCount.ToString();
    }
}
