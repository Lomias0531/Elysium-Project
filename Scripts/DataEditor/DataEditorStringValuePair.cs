using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DataEditorStringValuePair : MonoBehaviour
{
    public InputField ipt_Search;
    public Dropdown dpd_String;
    public InputField ipt_Value;
    public Button btn_DeleteThis;
    StringIndexType searchType;
    Dictionary<string, string> curSelectedNames = new Dictionary<string, string>();
    Dictionary<string, string> searchResult = new Dictionary<string, string>();
    // Start is called before the first frame update
    void Start()
    {
        btn_DeleteThis.onClick.AddListener(RemoveThis);
        ipt_Search.onValueChanged.AddListener(GetSearchResult);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitThis(StringIndexType type)
    {
        curSelectedNames.Clear();

        searchType = type;

        string folderName;

        switch (searchType)
        {
            default:
                {
                    folderName = "";
                    break;
                }
            case StringIndexType.Components:
                {
                    folderName = "Components";
                    break;
                }
        }

        var dicPath = Application.dataPath + "/Resources/ScriptableItems/" + folderName + "/";
        var folderInfo = new DirectoryInfo(dicPath).GetFiles("*.json").ToList();
        var files = folderInfo.Select(x => x.Name).ToList();
        var names = files.Select(x => x.Split('.')[0]).ToList();

        foreach (var name in names)
        {
            var json = File.ReadAllText(dicPath + name + ".json");

            switch(searchType)
            {
                default:
                    {
                        break;
                    }
                case StringIndexType.Components:
                    {
                        var thisData = JsonConvert.DeserializeObject<ComponentData>(json);
                        curSelectedNames.Add(name, thisData.ComponentName);
                        break;
                    }
            }
        }
        GetSearchResult("");
    }
    public void GetSearchResult(string str)
    {
        searchResult.Clear();
        foreach (var item in curSelectedNames)
        {
            if(item.Value.Contains(str))
            {
                searchResult.Add(item.Key, item.Value);
            }
        }
        dpd_String.ClearOptions();
        dpd_String.AddOptions(searchResult.Values.ToList());
    }
    void RemoveThis()
    {

    }
    public EditorStringValuePair GetThisValue()
    {
        EditorStringValuePair result = new EditorStringValuePair()
        {
            str = searchResult.Keys.ToList()[dpd_String.value],
            val = float.Parse(ipt_Value.text)
        };
        return result;
    }
}
public struct EditorStringValuePair
{
    public string str;
    public float val;
}