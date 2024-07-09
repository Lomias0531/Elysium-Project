using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataController : Singletion<DataController>
{
    public SO_ItemData GetItemInfo(string itemID)
    {
        var item = Resources.Load<SO_ItemData>("ScriptableItems/Items/" + itemID);
        return item;
    }
    public SO_ComponentData GetComponentData(string compID)
    {
        return Resources.Load<SO_ComponentData>("ScriptableItems/Components/" + compID);
    }
    public GameObject GetEntityViaID(string entityID)
    {
        var entityItem = Resources.Load<SO_EntityData>("ScriptableItems/Entities/" +  entityID);
        return entityItem.EntityObject;
    }
}
