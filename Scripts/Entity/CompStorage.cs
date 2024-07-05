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
        PlayerController.Instance.GetInteractRange(InteractFunction.Store);
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
                        inventory[index].stackCount += receivedItem.stackCount;
                        receivedItem.stackCount = 0;
                    }
                    else
                    {
                        var stackDiv = itemInfo.maxStackCount - inventory[index].stackCount;
                        inventory[index].stackCount = itemInfo.maxStackCount;
                        receivedItem.stackCount -= stackDiv;
                    }
                }
            }

            index++;
        } while (receivedItem.stackCount > 0 && index <= maxStorageSlot);

        return receivedItem;
    }
    public ItemData TransferItem(ItemData transferedItem)
    {
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].itemID == transferedItem.itemID)
            {
                if (inventory[i].stackCount >= transferedItem.stackCount)
                {
                    inventory[i].stackCount -= transferedItem.stackCount;
                    transferedItem.stackCount = 0;
                }
                else
                {
                    transferedItem.stackCount -= inventory[i].stackCount;
                    inventory[i].stackCount = 0;
                    inventory.RemoveAt(i);
                }
            }
        }
        return transferedItem;
    }
}
[Serializable]
public class ItemData
{
    public string itemID;
    public int stackCount;
}