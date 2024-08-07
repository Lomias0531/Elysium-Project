using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class DataEditorMain : MonoBehaviour
{
    public Dictionary<string, CanvasGroup> editorPages = new Dictionary<string, CanvasGroup>();
    public Button btn_LoadComponents;
    public Button btn_LoadEntities;
    public Button btn_LoadItems;

    public Dropdown dpd_SelectItem;
    public InputField ipt_Filter;

    public Button btn_Save;
    public Button btn_Delete;
    public Button btn_New;

    public string curDic;
    Dictionary<string,string> curEditNames = new Dictionary<string, string>();
    Dictionary<string,string> searchResults = new Dictionary<string, string>();
    [Space(5)]
    [Header("Components")]
    public CanvasGroup canvas_Components;
    public InputField ipt_CompID;
    public InputField ipt_CompName;
    public InputField ipt_CompEndurance;
    public InputField ipt_CompEnergy;
    public Dropdown dpd_CompType;
    public Transform tsf_FunctionsContainer;
    public CompFunctionsItem compFunctionsItem;
    public Button btn_AddFunction;
    public Button btn_RemoveFunction;
    List<CompFunctionsItem> compFunctionsItems = new List<CompFunctionsItem>();
    public Dictionary<string,CanvasGroup> functionPages = new Dictionary<string, CanvasGroup>();
    SO_ComponentData curEditComponent;
    CompFunctionsItem curSelectedFunction;
    [Space(2)]
    [Header("FunctionsCommon")]
    public Button btn_ConfirmFunctionEdit;
    public Button btn_CancelFunctinEdit;
    public InputField ipt_FunctionName;
    public Button btn_SelectIcon;
    public Image img_Icon;
    public InputField ipt_ApplyTimeInterval;
    public InputField ipt_FunctionValue;
    public InputField ipt_FunctionConsume;
    public Toggle tog_Auto;
    public Text txt_ValueDesc;
    [Space(1)]
    [Header("Mobile Components")]
    public CanvasGroup canvas_CompMobile;
    public Dropdown dpd_MoveType;
    public Dropdown dpd_MoveStyle;
    // Start is called before the first frame update
    void Start()
    {
        BindKeys();
        LoadComponentsData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void BindKeys()
    {
        btn_LoadComponents.onClick.AddListener(LoadComponentsData);
        btn_LoadEntities.onClick.AddListener(LoadEntitiesData);
        btn_LoadItems.onClick.AddListener(LoadItemsData);
        ipt_Filter.onValueChanged.AddListener(InputSearchFilter);
        btn_Save.onClick.AddListener(SaveEditContent);
        btn_Delete.onClick.AddListener(DeleteCurContent);
        btn_New.onClick.AddListener(AddNewContent);
        dpd_SelectItem.onValueChanged.AddListener(LoadSelectedIndex);
        btn_AddFunction.onClick.AddListener(AddFunction);
        btn_RemoveFunction.onClick.AddListener(RemoveFunction);
        btn_ConfirmFunctionEdit.onClick.AddListener(ConfirmFunctionEdit);
        btn_CancelFunctinEdit.onClick.AddListener(CancelFunctionEdit);
    }
    void LoadComponentsData()
    {
        LoadData("Components");
    }
    void LoadEntitiesData()
    {
        LoadData("Entities");
    }
    void LoadItemsData()
    {
        LoadData("Items");
    }
    void LoadData(string type)
    {
        ipt_Filter.text = "";
        curEditNames.Clear();
        var dicPath = Application.dataPath + "/Resources/ScriptableItems/" + type + "/";
        var folderInfo = new DirectoryInfo(dicPath).GetFiles("*.json").ToList();
        var files = folderInfo.Select(x => x.Name).ToList();
        var names = files.Select(x => x.Split('.')[0]).ToList();
        foreach (var name in names)
        {
            switch(type)
            {
                default:
                    {
                        break;
                    }
                case "Components":
                    {
                        var json = File.ReadAllText(dicPath + name);
                        var thisData = JsonConvert.DeserializeObject<ComponentData>(json);
                        curEditNames.Add(name, thisData.ComponentName);
                        break;
                    }
            }
        }
        curDic = type;
        InputSearchFilter("");
        LoadSelectedIndex(0);

        dpd_CompType.ClearOptions();
        foreach (var types in System.Enum.GetNames(typeof(ComponentFunctionType)))
        {
            dpd_CompType.options.Add(new Dropdown.OptionData() { text = types });
        }
    }
    void InputSearchFilter(string name)
    {
        dpd_SelectItem.ClearOptions();
        searchResults.Clear();
        foreach (var item in curEditNames)
        {
            if(item.Value.Contains(name))
            {
                searchResults.Add(item.Key, item.Value);
            }
        }
        dpd_SelectItem.AddOptions(searchResults.Values.ToList());
    }
    void LoadSelectedIndex(int index)
    {
        if (dpd_SelectItem.options.Count <= 0) return;
        switch(curDic)
        {
            default:
                {
                    break;
                }
            case "Components":
                {
                    foreach (var item in compFunctionsItems)
                    {
                        Destroy(item.gameObject);
                    }
                    compFunctionsItems.Clear();

                    var selectedID = searchResults.Keys.ToList()[index];

                    curEditComponent = DataController.Instance.GetComponentData(selectedID);

                    ipt_CompID.text = curEditComponent.ComponentID;
                    if (string.IsNullOrEmpty(curEditComponent.ComponentID))
                    {
                        ipt_CompID.text = "Comp" + Tools.GetTimeStamp();
                    }
                    ipt_CompName.text = curEditComponent.ComponentName;
                    ipt_CompEndurance.text = curEditComponent.ComponentEndurance.ToString();
                    ipt_CompEnergy.text = curEditComponent.ComponentInternalBattery.ToString();
                    dpd_CompType.captionText.text = curEditComponent.componentType.ToString();
                    foreach (var function in curEditComponent.functions)
                    {
                        var functionItem = Instantiate(compFunctionsItem);
                        functionItem.transform.SetParent(tsf_FunctionsContainer.transform);
                        functionItem.gameObject.SetActive(true);
                        functionItem.InitThis(function);

                        compFunctionsItems.Add(functionItem);
                    }
                    break;
                }
            case "Entities":
                {
                    break;
                }
            case "Items":
                {
                    break;
                }
        }
    }
    void SaveEditContent()
    {
        ComponentData newComponentData = new ComponentData();
        newComponentData.ComponentID = ipt_CompID.text;
        newComponentData.ComponentName = ipt_CompName.text;
        newComponentData.ComponentEndurance = float.Parse(ipt_CompEndurance.text);
        newComponentData.ComponentInternalBattery = float.Parse(ipt_CompEnergy.text);
        newComponentData.componentType = (ComponentFunctionType)dpd_CompType.value;
        List<CompFunctionDetail> details = new List<CompFunctionDetail>();
        foreach (var func in compFunctionsItems)
        {
            details.Add(func.GetThisFunction());
        }
        newComponentData.functions = details.ToArray();

        var json = JsonConvert.SerializeObject(newComponentData);
        
        var dic = Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/";
        File.WriteAllText(dic + newComponentData.ComponentID + ".json", json);
    }
    void AddNewContent()
    {

    }
    void DeleteCurContent()
    {

    }
    public void LoadCompFunctionDetail(CompFunctionsItem function)
    {
        curSelectedFunction = function;
        foreach (var funcItems in compFunctionsItems)
        {
            if(funcItems != function)
            {
                funcItems.TriggerSelection(false);
            }else
            {
                funcItems.TriggerSelection(true);
            }
        }
    }
    void AddFunction()
    {

    }
    void RemoveFunction()
    {

    }
    void ConfirmFunctionEdit()
    {
        CompFunctionDetail newFunction = new CompFunctionDetail();
        switch (curEditComponent.componentType)
        {
            default:
                {
                    break;
                }
            case ComponentFunctionType.Mobile:
                {
                    newFunction.functionName = ipt_FunctionName.text;
                    newFunction.functionIcon = img_Icon.sprite;
                    newFunction.functionApplyTimeInterval = float.Parse(ipt_ApplyTimeInterval.text);
                    newFunction.functionValue = float.Parse(ipt_FunctionValue.text);
                    newFunction.functionConsume = float.Parse(ipt_FunctionConsume.text);
                    newFunction.canBeAuto = tog_Auto.isOn;
                    newFunction.functionIntVal = new int[2]
                    {
                        dpd_MoveType.value,
                        dpd_MoveStyle.value,
                    };
                    break;
                }
        }
        curSelectedFunction.InitThis(newFunction);
    }
    void CancelFunctionEdit()
    {

    }
}
public struct ComponentData
{
    public string ComponentID;
    public string ComponentName;
    public float ComponentEndurance;
    public float ComponentInternalBattery;
    public bool isFatalComponent;
    public ComponentFunctionType componentType;
    public CompFunctionDetail[] functions;
    public string ComponentDescription;
}
[Serializable]
public struct CompFunctionDetail
{
    public string functionName;
    public Sprite functionIcon;
    public float functionApplyTimeInterval;
    public float functionValue;
    public float functionConsume;
    public bool canBeAuto;
    public bool isAuto;
    public int[] functionIntVal;
    public float[] functionFloatVal;
    public bool[] functionBoolVal;
    public string[] functionStringVal;
    public string functionDescription;
}
public enum ComponentFunctionType
{
    Mobile,
    Weapon,
    Interact,
    None,
}