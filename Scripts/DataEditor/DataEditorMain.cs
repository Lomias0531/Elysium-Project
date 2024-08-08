using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEditor;

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
    public InputField ipt_ComponentDescription;
    public Transform tsf_FunctionsContainer;
    public CompFunctionsItem compFunctionsItem;
    public Button btn_AddFunction;
    public Button btn_RemoveFunction;
    List<CompFunctionsItem> compFunctionsItems = new List<CompFunctionsItem>();
    public Dictionary<ComponentFunctionType, CanvasGroup> functionPages = new Dictionary<ComponentFunctionType, CanvasGroup>();
    ComponentData curEditComponent;
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
    public Text txt_FunctionValueDesc;
    public InputField ipt_FunctionDesc;
    public Sprite defaultIcon;
    public string curSelectedIconPath;
    public int curSelectedIconIndex;
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
        AddPages();
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
        dpd_CompType.onValueChanged.AddListener(OnComponentTypeChanged);
        btn_AddFunction.onClick.AddListener(AddFunction);
        btn_RemoveFunction.onClick.AddListener(RemoveFunction);
        btn_ConfirmFunctionEdit.onClick.AddListener(ConfirmFunctionEdit);
        btn_CancelFunctinEdit.onClick.AddListener(CancelFunctionEdit);
        btn_SelectIcon.onClick.AddListener(SelectComponentIcon);
    }
    void AddPages()
    {
        editorPages.Add("Components", canvas_Components);
        functionPages.Add(ComponentFunctionType.Mobile, canvas_CompMobile);
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

        foreach (var item in editorPages)
        {
            item.Value.alpha = 0;
            item.Value.interactable = false;
            item.Value.blocksRaycasts = false;
        }
        if(editorPages.ContainsKey(type))
        {
            editorPages[type].alpha = 1;
            editorPages[type].interactable = true;
            editorPages[type].blocksRaycasts = true;
        }

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
                        var json = File.ReadAllText(dicPath + name + ".json");
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

                    //curEditComponent = DataController.Instance.GetComponentData(selectedID);
                    var json = File.ReadAllText(Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/" + selectedID + ".json");
                    var data = JsonConvert.DeserializeObject<ComponentData>(json);

                    LoadComponentData(data);
                    //curEditComponent = data;

                    //ipt_CompID.text = curEditComponent.ComponentID;
                    //if (string.IsNullOrEmpty(curEditComponent.ComponentID))
                    //{
                    //    ipt_CompID.text = "Comp" + Tools.GetTimeStamp();
                    //}
                    //ipt_CompName.text = curEditComponent.ComponentName;
                    //ipt_CompEndurance.text = curEditComponent.ComponentEndurance.ToString();
                    //ipt_CompEnergy.text = curEditComponent.ComponentInternalBattery.ToString();
                    //dpd_CompType.captionText.text = curEditComponent.componentType.ToString();
                    //foreach (var function in curEditComponent.functions)
                    //{
                    //    var functionItem = Instantiate(compFunctionsItem);
                    //    functionItem.transform.SetParent(tsf_FunctionsContainer.transform);
                    //    functionItem.gameObject.SetActive(true);
                    //    functionItem.InitThis(function);

                    //    compFunctionsItems.Add(functionItem);
                    //}

                    //if(curEditComponent.componentType == ComponentFunctionType.None)
                    //{
                    //    btn_AddFunction.interactable = false;
                    //    btn_RemoveFunction.interactable = false;
                    //    btn_ConfirmFunctionEdit.interactable = false;
                    //    btn_CancelFunctinEdit.interactable = false;
                    //}else
                    //{
                    //    btn_AddFunction.interactable = true;
                    //    btn_RemoveFunction.interactable = true;
                    //    btn_ConfirmFunctionEdit.interactable = true;
                    //    btn_CancelFunctinEdit.interactable = true;
                    //}
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
    void LoadComponentData(ComponentData data)
    {
        curEditComponent = data;
        ipt_CompID.text = curEditComponent.ComponentID;
        if (string.IsNullOrEmpty(curEditComponent.ComponentID))
        {
            ipt_CompID.text = "Comp" + Tools.GetTimeStamp();
        }
        ipt_CompName.text = curEditComponent.ComponentName;
        ipt_CompEndurance.text = curEditComponent.ComponentEndurance.ToString();
        ipt_CompEnergy.text = curEditComponent.ComponentInternalBattery.ToString();
        dpd_CompType.captionText.text = curEditComponent.componentType.ToString();
        ipt_ComponentDescription.text = curEditComponent.ComponentDescription;
        if(curEditComponent.functions != null)
        {
            foreach (var function in curEditComponent.functions)
            {
                var functionItem = Instantiate(compFunctionsItem);
                functionItem.transform.SetParent(tsf_FunctionsContainer.transform);
                functionItem.gameObject.SetActive(true);
                functionItem.InitThis(function);

                compFunctionsItems.Add(functionItem);
            }
        }

        if (curEditComponent.componentType == ComponentFunctionType.None)
        {
            btn_AddFunction.interactable = false;
            btn_RemoveFunction.interactable = false;
            btn_ConfirmFunctionEdit.interactable = false;
            btn_CancelFunctinEdit.interactable = false;
        }
        else
        {
            btn_AddFunction.interactable = true;
            btn_RemoveFunction.interactable = true;
            btn_ConfirmFunctionEdit.interactable = false;
            btn_CancelFunctinEdit.interactable = false;
        }

        CancelFunctionEdit();
    }
    void SaveEditContent()
    {
        ComponentData newComponentData = new ComponentData();
        newComponentData.ComponentID = ipt_CompID.text;
        newComponentData.ComponentName = ipt_CompName.text;
        newComponentData.ComponentEndurance = float.Parse(ipt_CompEndurance.text);
        newComponentData.ComponentInternalBattery = float.Parse(ipt_CompEnergy.text);
        newComponentData.componentType = (ComponentFunctionType)dpd_CompType.value;
        newComponentData.ComponentDescription = ipt_ComponentDescription.text;
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

    void OnComponentTypeChanged(int index)
    {
        ComponentFunctionType type = (ComponentFunctionType)index;
        curEditComponent.componentType = type;
        if(type == ComponentFunctionType.None)
        {
            btn_AddFunction.interactable = false;
            btn_RemoveFunction.interactable = false;
            btn_ConfirmFunctionEdit.interactable = false;
            btn_CancelFunctinEdit.interactable = false;
        }
        else
        {
            btn_AddFunction.interactable = true;
            btn_RemoveFunction.interactable = true;
            btn_ConfirmFunctionEdit.interactable = false;
            btn_CancelFunctinEdit.interactable = false;
        }
    }
    void AddNewContent()
    {
        ComponentData data = new ComponentData();
        data.ComponentID = "Comp" + Tools.GetTimeStamp();
        data.componentType = ComponentFunctionType.None;
        dpd_CompType.value = -1;

        LoadComponentData(data);
    }
    void DeleteCurContent()
    {
        var path = Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/" + curEditComponent.ComponentID + ".json";
        if(File.Exists(path))
        {
            File.Delete(path);
        }
        AddNewContent();
    }
    public void LoadCompFunctionDetail(CompFunctionsItem function)
    {
        foreach (var page in functionPages)
        {
            page.Value.alpha = 0;
            page.Value.interactable = false;
            page.Value.blocksRaycasts = false;
        }
        if (functionPages.ContainsKey(curEditComponent.componentType))
        {
            functionPages[curEditComponent.componentType].alpha = 1;
            functionPages[curEditComponent.componentType].interactable = true;
            functionPages[curEditComponent.componentType].blocksRaycasts = true;
        }

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

        switch(curEditComponent.componentType)
        {
            default:
                {
                    break;
                }
            case ComponentFunctionType.Mobile:
                {
                    dpd_MoveStyle.ClearOptions();
                    foreach (var item in Enum.GetNames(typeof(BaseObj.MoveStyle)))
                    {
                        dpd_MoveStyle.options.Add(new Dropdown.OptionData() { text = item });
                    }
                    dpd_MoveType.ClearOptions();
                    foreach (var item in Enum.GetNames(typeof(BaseObj.MoveType)))
                    {
                        dpd_MoveType.options.Add(new Dropdown.OptionData() { text = item });
                    }

                    var func = function.GetThisFunction();
                    ipt_FunctionName.text = func.functionName;
                    if(string.IsNullOrEmpty(func.functionIconPath))
                    {
                        img_Icon.sprite = defaultIcon;
                    }else
                    {
                        object[] sp = AssetDatabase.LoadAllAssetsAtPath(func.functionIconPath);
                        img_Icon.sprite = (Sprite)sp[func.functionIconIndex];
                    }
                    ipt_ApplyTimeInterval.text = func.functionApplyTimeInterval.ToString();
                    txt_FunctionValueDesc.text = "ÒÆ¶¯Á¦";
                    ipt_FunctionValue.text = func.functionValue.ToString();
                    ipt_FunctionConsume.text = func.functionConsume.ToString();
                    tog_Auto.isOn = func.canBeAuto;
                    dpd_MoveType.value = func.functionIntVal[0];
                    dpd_MoveStyle.value = func.functionIntVal[1];
                    ipt_FunctionDesc.text = func.functionDescription;
                    break;
                }
        }

        btn_ConfirmFunctionEdit.interactable = true;
        btn_CancelFunctinEdit.interactable = true;
    }
    void AddFunction()
    {
        CompFunctionDetail function = new CompFunctionDetail();

        switch(curEditComponent.componentType)
        {
            default:
                {
                    break;
                }
            case ComponentFunctionType.Mobile:
                {
                    function.functionIntVal = new int[2];
                    break;
                }
        }

        var functionItem = Instantiate(compFunctionsItem);
        functionItem.transform.SetParent(tsf_FunctionsContainer.transform);
        functionItem.gameObject.SetActive(true);
        functionItem.InitThis(function);
        img_Icon.sprite = defaultIcon;

        compFunctionsItems.Add(functionItem);

        LoadCompFunctionDetail(functionItem);
    }
    void RemoveFunction()
    {
        if(compFunctionsItems.Contains(curSelectedFunction))
        {
            Destroy(curSelectedFunction.gameObject);
            compFunctionsItems.Remove(curSelectedFunction);
        }
        CancelFunctionEdit();
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
                    newFunction.functionIconPath = curSelectedIconPath;
                    newFunction.functionIconIndex = curSelectedIconIndex;
                    newFunction.functionApplyTimeInterval = float.Parse(ipt_ApplyTimeInterval.text);
                    newFunction.functionValue = float.Parse(ipt_FunctionValue.text);
                    newFunction.functionConsume = float.Parse(ipt_FunctionConsume.text);
                    newFunction.canBeAuto = tog_Auto.isOn;
                    newFunction.functionIntVal = new int[2]
                    {
                        dpd_MoveType.value,
                        dpd_MoveStyle.value,
                    };
                    newFunction.functionDescription = ipt_FunctionDesc.text;
                    break;
                }
        }
        curSelectedFunction.InitThis(newFunction);
    }
    void CancelFunctionEdit()
    {
        foreach (var page in functionPages)
        {
            page.Value.alpha = 0;
            page.Value.interactable = false;
            page.Value.blocksRaycasts = false;
        }

        ipt_FunctionName.text = "";
        img_Icon.sprite = defaultIcon;
        ipt_ApplyTimeInterval.text = "";
        ipt_FunctionValue.text = "";
        ipt_FunctionConsume.text = "";
        ipt_FunctionDesc.text = "";

        btn_ConfirmFunctionEdit.interactable = false;
        btn_CancelFunctinEdit.interactable = false;
    }
    void SelectComponentIcon()
    {
        var dir = Application.dataPath + "/Images/Icons/";
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
    public string functionIconPath;
    public int functionIconIndex;
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