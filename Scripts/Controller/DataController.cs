using Mono.Cecil;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEditor.Progress;

public class DataController : Singletion<DataController>
{
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
            var obj = Resources.Load<GameObject>("Prefabs/Characters/" + data.EntityIndex);
            var target = obj.gameObject.GetComponent<BaseUnit>();
            target.thisEntityData = data;
            return target;
        }
        return null;
    }
    public BaseUnit GetConstructData(string entityID)
    {
        var path = Application.dataPath + "/Resources/ScriptableItems/Entities/" + entityID + ".json";
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<EntityData>(json);
            var obj = Resources.Load<GameObject>("Prefabs/Entities/Construction/" + data.EntityIndex);
            var target = obj.gameObject.GetComponent<BaseUnit>();
            target.thisEntityData = data;
            return target;
        }
        return null;
    }
}
