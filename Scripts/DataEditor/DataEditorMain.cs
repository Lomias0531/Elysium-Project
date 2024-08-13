using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEditor;
using DG.Tweening.Plugins.Core.PathCore;

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
    public InputField ipt_CompDefense;
    public Dropdown dpd_CompType;
    public Toggle tog_isCompFatal;
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
    [Space(1)]
    [Header("Icon Selector")]
    public CanvasGroup canvas_IconSelector;
    public Transform tsf_IconContainer;
    public IconSelectorItem iconSelectorItem;
    public Button btn_ConfirmIcon;
    public Button btn_CancelIcon;
    IconSelectorItem selectedIcon;
    List<IconSelectorItem> iconSelectorItems = new List<IconSelectorItem>();
    [Space(1)]
    [Header("Mobile Components")]
    public CanvasGroup canvas_CompMobile;
    public Dropdown dpd_MoveType;
    public Dropdown dpd_MoveStyle;
    [Space(1)]
    [Header("Weapon Components")]
    public CanvasGroup canvas_CompWeapon;
    public InputField ipt_MaxRange;
    public InputField ipt_MinRange;
    public InputField ipt_BlastRange;
    public Dropdown dpd_WeapenBallisticType;
    public InputField ipt_BulletsCount;
    public InputField ipt_BulletsInterval;
    public InputField ipt_BulletSpeed;
    public InputField ipt_TrailParticle;
    public InputField ipt_BlastParticle;
    [Space(1)]
    [Header("Constructor Components")]
    public CanvasGroup canvas_Constructor;
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
        btn_ConfirmIcon.onClick.AddListener(OnConfirmIcon);
        btn_CancelIcon.onClick.AddListener(OnCancelIcon);
    }
    void AddPages()
    {
        editorPages.Add("Components", canvas_Components);
        functionPages.Add(ComponentFunctionType.Mobile, canvas_CompMobile);
        functionPages.Add(ComponentFunctionType.Weapon, canvas_CompWeapon);
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
        ipt_CompDefense.text = curEditComponent.ComponentDefense.ToString();
        dpd_CompType.captionText.text = curEditComponent.componentType.ToString();
        ipt_ComponentDescription.text = curEditComponent.ComponentDescription;
        tog_isCompFatal.isOn = curEditComponent.isFatalComponent;
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
        newComponentData.ComponentDefense = float.Parse(ipt_CompDefense.text);
        newComponentData.componentType = (ComponentFunctionType)dpd_CompType.value;
        newComponentData.ComponentDescription = ipt_ComponentDescription.text;
        newComponentData.isFatalComponent = tog_isCompFatal.isOn;
        List<CompFunctionDetail> details = new List<CompFunctionDetail>();
        foreach (var func in compFunctionsItems)
        {
            details.Add(func.GetThisFunction());
        }
        newComponentData.functions = details.ToArray();

        var json = JsonConvert.SerializeObject(newComponentData);
        
        var dic = Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/";
        File.WriteAllText(dic + newComponentData.ComponentID + ".json", json);

        LoadData(curDic);
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

        var func = function.GetThisFunction();
        ipt_FunctionName.text = func.functionName;
        if (string.IsNullOrEmpty(func.functionIconPath))
        {
            img_Icon.sprite = defaultIcon;
        }
        else
        {
            img_Icon.sprite = Tools.GetIcon(func.functionIconPath, func.functionIconIndex);
        }
        ipt_ApplyTimeInterval.text = func.functionApplyTimeInterval.ToString();
        ipt_FunctionValue.text = func.functionValue.ToString();
        ipt_FunctionConsume.text = func.functionConsume.ToString();
        tog_Auto.isOn = func.canBeAuto;
        ipt_FunctionDesc.text = func.functionDescription;

        switch (curEditComponent.componentType)
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

                    txt_FunctionValueDesc.text = "ÒÆ¶¯Á¦";

                    dpd_MoveType.value = func.functionIntVal[0];
                    dpd_MoveStyle.value = func.functionIntVal[1];
                    break;
                }
            case ComponentFunctionType.Weapon:
                {
                    txt_FunctionValueDesc.text = "ÉËº¦";

                    dpd_WeapenBallisticType.ClearOptions();
                    foreach (var item in Enum.GetNames(typeof(CompWeapon.WeaponAttackType)))
                    {
                        dpd_WeapenBallisticType.options.Add(new Dropdown.OptionData() { text = item });
                    }

                    ipt_MinRange.text = func.functionIntVal[0].ToString();
                    ipt_MaxRange.text = func.functionIntVal[1].ToString();
                    ipt_BlastRange.text = func.functionIntVal[2].ToString();
                    dpd_WeapenBallisticType.value = func.functionIntVal[3];
                    ipt_BulletsCount.text = func.functionIntVal[4].ToString();
                    ipt_BulletsInterval.text = func.functionFloatVal[0].ToString();
                    ipt_BulletSpeed.text = func.functionFloatVal[1].ToString();

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
            case ComponentFunctionType.Weapon:
                {
                    function.functionIntVal = new int[5];
                    function.functionFloatVal = new float[2];
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
        newFunction.functionName = ipt_FunctionName.text;
        if(selectedIcon != null)
        {
            newFunction.functionIconPath = selectedIcon.iconName;
            newFunction.functionIconIndex = selectedIcon.iconIndex;
        }else
        {
            newFunction.functionIconPath = "";
            newFunction.functionIconIndex = -1;
        }
        newFunction.functionApplyTimeInterval = float.Parse(ipt_ApplyTimeInterval.text);
        newFunction.functionValue = float.Parse(ipt_FunctionValue.text);
        newFunction.functionConsume = float.Parse(ipt_FunctionConsume.text);
        newFunction.canBeAuto = tog_Auto.isOn;
        newFunction.functionDescription = ipt_FunctionDesc.text;

        switch (curEditComponent.componentType)
        {
            default:
                {
                    break;
                }
            case ComponentFunctionType.Mobile:
                {
                    newFunction.functionIntVal = new int[2]
                    {
                        dpd_MoveType.value,
                        dpd_MoveStyle.value,
                    };
                    break;
                }
            case ComponentFunctionType.Weapon:
                {
                    newFunction.functionIntVal = new int[5]
                    {
                        int.Parse(ipt_MinRange.text),
                        int.Parse(ipt_MaxRange.text),
                        int.Parse(ipt_BlastRange.text),
                        dpd_WeapenBallisticType.value,
                        int.Parse(ipt_BulletsCount.text),
                    };
                    newFunction.functionFloatVal = new float[2]
                    {
                        float.Parse(ipt_BulletsInterval.text),
                        float.Parse(ipt_BulletSpeed.text),
                    };
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
        tog_Auto.isOn = false;
        img_Icon.sprite = defaultIcon;

        btn_ConfirmFunctionEdit.interactable = false;
        btn_CancelFunctinEdit.interactable = false;
    }
    void SelectComponentIcon()
    {
        TriggerSelectIcon();
    }
    void TriggerSelectIcon()
    {
        canvas_IconSelector.alpha = 1;
        canvas_IconSelector.blocksRaycasts = true;
        canvas_IconSelector.interactable = true;

        foreach (var item in iconSelectorItems)
        {
            Destroy(item.gameObject);
        }
        iconSelectorItems.Clear();

        var dir = Application.dataPath + "/Resources/Images/Icons/";

        var folderInfo = new DirectoryInfo(dir).GetFiles("*.png").ToList();
        var files = folderInfo.Select(x => x.Name).ToList();
        var names = files.Select(x => x.Split('.')[0]).ToList();

        foreach (var item in names)
        {
            //object[] sp = Resources.LoadAll<Sprite>("Images/Icons");
            object[] sp = AssetDatabase.LoadAllAssetRepresentationsAtPath("Assets/Resources/Images/Icons/" + item + ".png");
            for(int i = 0;i<sp.Length;i++)
            {
                var iconItem = Instantiate(iconSelectorItem);
                iconItem.IntThis(item, i, this);
                iconItem.transform.SetParent(tsf_IconContainer);
                iconItem.gameObject.SetActive(true);
                iconSelectorItems.Add(iconItem);
            }
        }
    }
    void OnConfirmIcon()
    {
        canvas_IconSelector.alpha = 0;
        canvas_IconSelector.blocksRaycasts = false;
        canvas_IconSelector.interactable = false;

        if(selectedIcon != null)
            img_Icon.sprite = Tools.GetIcon(selectedIcon.iconName, selectedIcon.iconIndex);
    }
    void OnCancelIcon()
    {
        selectedIcon = null;

        canvas_IconSelector.alpha = 0;
        canvas_IconSelector.blocksRaycasts = false;
        canvas_IconSelector.interactable = false;
    }
    public void OnConfirmSelectIconItem(IconSelectorItem item)
    {
        selectedIcon = item;
        foreach (var iconItem in iconSelectorItems)
        {
            if(iconItem == item)
            {
                iconItem.TriggerSelected(true);
            }else
            {
                iconItem.TriggerSelected(false);
            }
        }
    }
}
public struct ComponentData
{
    public string ComponentID;
    public string ComponentName;
    public float ComponentEndurance;
    public float ComponentInternalBattery;
    public float ComponentDefense;
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
    Build,
    Construct,
    Harvest,
    Generator,
    PowerDispatcher,
}