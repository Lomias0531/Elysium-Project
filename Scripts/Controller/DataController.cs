using Mono.Cecil;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEditor.Progress;

public class DataController : Singletion<DataController>
{
    //public SO_ItemData GetItemInfo(string itemID)
    //{
    //    var item = Resources.Load<SO_ItemData>("ScriptableItems/Items/" + itemID);
    //    return item;
    //}
    //public SO_ComponentData GetComponentData(string compID)
    //{
    //    return Resources.Load<SO_ComponentData>("ScriptableItems/Components/" + compID);
    //}
    //public BaseObj GetEntityViaID(string entityID)
    //{
    //    var entityItem = Resources.Load<SO_EntityData>("ScriptableItems/Entities/" +  entityID);
    //    return entityItem.EntityObject;
    //}
    public ItemDataEditor GetItemData(string itemID)
    {
        var path = Application.dataPath + "/Resources/ScriptableItems/Items/" + itemID + ".json";
        if(File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<ItemDataEditor>(json);
            return data;
        }
        return new ItemDataEditor();
    }
    public ComponentData GetComponentData(string compID)
    {
        var path = Application.dataPath + "/Resources/ScriptableItems/Components/" + compID + ".json";
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<ComponentData>(json);
            return data;
        }
        return new ComponentData();
    }
    public BaseUnit GetEntityData(string entityID)
    {
        var path = Application.dataPath + "/Resources/ScriptableItems/Entities/" + entityID + ".json";
        if(File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<EntityData>(json);
            var obj = Resources.Load<GameObject>("Prefabs/Characters/" + entityID);
            var target = obj.gameObject.GetComponent<BaseUnit>();
            target.thisEntityData = data;
            return target;
        }
        return null;
    }
}
