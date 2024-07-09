using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompStorage : BaseComponent
{
    public int maxStorageSlot;
    public List<ItemData> inventory = new List<ItemData>();
    public override void OnApply(int index)
    {
        //PlayerController.Instance.GetInteractRange(InteractFunction.Store);
        StartCoroutine(UIController.Instance.unitMenu.ShowEntityInventory());
    }

    public override void OnDestroyThis()
    {
        
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
    public ItemData ReceiveItem(ItemData receivedItem)
    {
        SO_ItemData itemInfo = DataController.Instance.GetItemInfo(receivedItem.itemID);

        int index = 0;
        do
        {
            if (index >= inventory.Count && index < maxStorageSlot)
            {
                var rec = new ItemData();
                rec.itemID = receivedItem.itemID;
                rec.stackCount = 0;
                inventory.Add(rec);
            }

            if(index < inventory.Count)
            {
                if (inventory[index].itemID == receivedItem.itemID)
                {
                    if (inventory[index].stackCount + receivedItem.stackCount <= itemInfo.maxStackCount)
                    {
                        SetInvCount(index, inventory[index].stackCount + receivedItem.stackCount);
                        receivedItem.stackCount = 0;
                    }
                    else
                    {
                        var stackDiv = itemInfo.maxStackCount - inventory[index].stackCount;
                        SetInvCount(index, itemInfo.maxStackCount);
                        receivedItem.stackCount -= stackDiv;
                    }
                }
            }

            index++;
        } while (receivedItem.stackCount > 0 && index <= maxStorageSlot);

        return receivedItem;
    }
    public ItemData TransferItem(CompStorage targetStorage, ItemData transferedItem)
    {
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].itemID == transferedItem.itemID)
            {
                if (inventory[i].stackCount <= transferedItem.stackCount)
                {
                    ItemData dataTemp = new ItemData();
                    dataTemp.itemID = inventory[i].itemID;
                    dataTemp.stackCount = inventory[i].stackCount;

                    var itemData = targetStorage.ReceiveItem(dataTemp);
                    transferedItem.stackCount -= inventory[i].stackCount;
                    if (itemData.stackCount <= 0)
                    {
                        SetInvCount(i, 0);
                    }else
                    {
                        SetInvCount(i,itemData.stackCount);
                    }
                }
                else
                {
                    var itemCount = transferedItem.stackCount;
                    var itemData = targetStorage.ReceiveItem(transferedItem);
                    var itemTransfered = itemCount - itemData.stackCount;

                    transferedItem.stackCount -= itemTransfered;
                    SetInvCount(i, inventory[i].stackCount - itemTransfered);
                }
            }
            if (inventory[i].stackCount <= 0)
            {
                inventory.RemoveAt(i);
            }
            if (transferedItem.stackCount <= 0) break;
        }
        return transferedItem;
    }
    public int GetItemCount(string itemID)
    {
        int result = 0;
        foreach (var item in inventory)
        {
            if(item.itemID == itemID)
            {
                result += item.stackCount;
            }
        }
        return result;
    }
    public void RemoveItem(ItemData itemInfo)
    {
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].itemID == itemInfo.itemID)
            {
                if (inventory[i].stackCount <= itemInfo.stackCount)
                {
                    ItemData dataTemp = new ItemData();
                    dataTemp.itemID = inventory[i].itemID;
                    dataTemp.stackCount = inventory[i].stackCount;

                    itemInfo.stackCount -= inventory[i].stackCount;
                    SetInvCount(i, 0);
                }
                else
                {
                    SetInvCount(i, inventory[i].stackCount - itemInfo.stackCount);
                    itemInfo.stackCount = 0;
                }
            }
            if (inventory[i].stackCount <= 0)
            {
                inventory.RemoveAt(i);
            }
            if (itemInfo.stackCount <= 0) break;
        }
    }
    void SetInvCount(int index, int count)
    {
        ItemData temp = new ItemData();
        temp.itemID = inventory[index].itemID;
        temp.stackCount = count;
        inventory[index] = temp;
    }
}
[Serializable]
public struct ItemData
{
    public string itemID;
    public int stackCount;
}