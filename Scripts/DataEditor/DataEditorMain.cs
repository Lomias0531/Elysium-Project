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
using Unity.VisualScripting;

public class DataEditorMain : MonoBehaviour
{
    public Dictionary<EditorPage, CanvasGroup> editorPages = new Dictionary<EditorPage, CanvasGroup>();
    public Button btn_LoadComponents;
    public Button btn_LoadEntities;
    public Button btn_LoadItems;

    public Dropdown dpd_SelectItem;
    public InputField ipt_Filter;

    public Button btn_Save;
    public Button btn_Delete;
    public Button btn_New;

    public EditorPage curDic;
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
    public InputField ipt_ComponentProductor;
    public Dropdown dpd_CompFuncType;
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
    public DataEditorStringValuePair stringValuePairItem;
    List<DataEditorStringValuePair> KeyValuePairItems = new List<DataEditorStringValuePair>();
    [Space(1)]
    [Header("Icon Selector")]
    public CanvasGroup canvas_IconSelector;
    public Transform tsf_IconContainer;
    public IconSelectorItem iconSelectorItem;
    public Button btn_ConfirmIcon;
    public Button btn_CancelIcon;
    //IconSelectorItem selectedIcon;
    string selectedIconPath = "";
    int selectedIconIndex = -1;
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
    public InputField ipt_ConstructItemID;
    public Button btn_AddKeyValuePair;
    public Transform tsf_constructorKeyValuePairContainer;
    [Space(1)]
    [Header("Builder Components")]
    public CanvasGroup canvas_Builder;
    public InputField ipt_BuildRange;
    public InputField ipt_BuildItemID;
    public Button btn_AddBuilderKeyValuePair;
    public Transform tsf_builderKeyValuePairContainer;
    [Space(1)]
    [Header("Power Dispatcher Components")]
    public CanvasGroup canvas_PowerDispatcher;
    public InputField ipt_MaxPowerDispatchable;
    [Space(1)]
    [Header("Resources Components")]
    public CanvasGroup canvas_Resource;
    public Dropdown dpd_RespurceType;
    [Space(1)]
    [Header("Entities")]
    public CanvasGroup canvas_Entities;
    public InputField ipt_EneityID;
    public InputField ipt_EntityName;
    public InputField ipt_EntityIndex;
    public Dropdown dpd_EntityType;
    EntityData curSelectedEntity;
    public InputField ipt_EntityProductor;
    public Button btn_AddPresetComponents;
    public Transform tsf_PresetComponentItemContainer;
    [Space(1)]
    [Header("Items")]
    public CanvasGroup canvas_Items;
    public InputField ipt_ItemID;
    public InputField ipt_ItemName;
    public InputField ipt_MaxStackCount;
    public Toggle tog_Consumeable;
    public Dropdown dpd_ItemType;
    ItemDataEditor curSelectedItem;
    public InputField ipt_ItemProductor;
    public Button btn_SelectItemIcon;
    public Image img_ItemIcon;
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
    #region Common
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
        btn_AddKeyValuePair.onClick.AddListener(AddConstructorStrValuePair);
        btn_AddBuilderKeyValuePair.onClick.AddListener(AddBuilderStrValuePair);
        btn_AddPresetComponents.onClick.AddListener(AddPresetComponentsStrValuePair);
        btn_SelectItemIcon.onClick.AddListener(SelectComponentIcon);
    }
    void AddPages()
    {
        editorPages.Add(EditorPage.Components, canvas_Components);
        functionPages.Add(ComponentFunctionType.Mobile, canvas_CompMobile);
        functionPages.Add(ComponentFunctionType.Weapon, canvas_CompWeapon);
        functionPages.Add(ComponentFunctionType.Construct, canvas_Constructor);
        functionPages.Add(ComponentFunctionType.Build, canvas_Builder);
        functionPages.Add(ComponentFunctionType.PowerDispatcher, canvas_PowerDispatcher);
        functionPages.Add(ComponentFunctionType.Resource, canvas_Resource);

        editorPages.Add(EditorPage.Entities, canvas_Entities);

        editorPages.Add(EditorPage.Items, canvas_Items);
    }
    void LoadComponentsData()
    {
        LoadData(EditorPage.Components);
    }
    void LoadEntitiesData()
    {
        LoadData(EditorPage.Entities);
    }
    void LoadItemsData()
    {
        LoadData(EditorPage.Items);
    }
    void LoadData(EditorPage type)
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
                case EditorPage.Components:
                    {
                        var json = File.ReadAllText(dicPath + name + ".json");
                        var thisData = JsonConvert.DeserializeObject<ComponentData>(json);
                        curEditNames.Add(name, thisData.ComponentName);
                        break;
                    }
                case EditorPage.Entities:
                    {
                        var json = File.ReadAllText(dicPath + name + ".json");
                        var thisData = JsonConvert.DeserializeObject<EntityData>(json);
                        curEditNames.Add(name, thisData.EntityName);
                        break;
                    }
                case EditorPage.Items:
                    {
                        var json = File.ReadAllText(dicPath + name + ".json");
                        var thisData = JsonConvert.DeserializeObject<ItemDataEditor>(json);
                        curEditNames.Add(name, thisData.ItemName);
                        break;
                    }
            }
        }

        switch (type)
        {
            default:
                {
                    break;
                }
            case EditorPage.Components:
                {
                    dpd_CompType.ClearOptions();
                    foreach (var types in System.Enum.GetNames(typeof(ComponentFunctionType)))
                    {
                        dpd_CompType.options.Add(new Dropdown.OptionData() { text = types });
                    }

                    dpd_CompFuncType.ClearOptions();
                    foreach (var types in Enum.GetNames(typeof(CompType)))
                    {
                        dpd_CompFuncType.options.Add(new Dropdown.OptionData() { text = types });
                    }
                    break;
                }
            case EditorPage.Entities:
                {
                    dpd_EntityType.ClearOptions();
                    foreach (var types in Enum.GetNames(typeof(EntityType)))
                    {
                        dpd_EntityType.options.Add(new Dropdown.OptionData() { text = types });
                    }
                    break;
                }
            case EditorPage.Items:
                {
                    dpd_ItemType.ClearOptions();
                    foreach (var types in Enum.GetNames(typeof(ItemType)))
                    {
                        dpd_ItemType.options.Add(new Dropdown.OptionData() { text = types });
                    }
                    break;
                }
        }
        curDic = type;
        InputSearchFilter("");
        LoadSelectedIndex(0);
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

        LoadSelectedIndex(0);
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
            case EditorPage.Components:
                {
                    foreach (var item in compFunctionsItems)
                    {
                        Destroy(item.gameObject);
                    }
                    compFunctionsItems.Clear();

                    var selectedID = searchResults.Keys.ToList()[index];

                    var json = File.ReadAllText(Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/" + selectedID + ".json");
                    var data = JsonConvert.DeserializeObject<ComponentData>(json);

                    LoadComponentData(data);
                    break;
                }
            case EditorPage.Entities:
                {
                    var selectedID = searchResults.Keys.ToList()[index];

                    var json = File.ReadAllText(Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/" + selectedID + ".json");
                    var data = JsonConvert.DeserializeObject<EntityData>(json);

                    LoadEntitiesData(data);
                    break;
                }
            case EditorPage.Items:
                {
                    var selectedID = searchResults.Keys.ToList()[index];

                    var json = File.ReadAllText(Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/" + selectedID + ".json");
                    var data = JsonConvert.DeserializeObject<ItemDataEditor>(json);

                    LoadItemsData(data);
                    break;
                }
        }
    }
    void SaveEditContent()
    {
        switch(curDic)
        {
            default:
                {
                    break;
                }
            case EditorPage.Components:
                {
                    SaveComponentsData();
                    break;
                }
            case EditorPage.Entities:
                {
                    SaveEneitiesData(); 
                    break;
                }
            case EditorPage.Items:
                {
                    SaveItemsData(); 
                    break;
                }
        }
    }
    void AddNewContent()
    {
        switch (curDic)
        {
            default:
                {
                    break;
                }
            case EditorPage.Components:
                {
                    ComponentData data = new ComponentData();
                    data.ComponentID = "Comp" + Tools.GetTimeStamp();
                    dpd_CompType.value = -1;

                    LoadComponentData(data);
                    OnComponentTypeChanged(0);
                    break;
                }
            case EditorPage.Entities:
                {
                    EntityData data = new EntityData();
                    data.EntityID = "Entity" + Tools.GetTimeStamp();
                    data.InstalledComponents = new string[0];
                    dpd_EntityType.value = -1;

                    LoadEntitiesData(data);
                    break;
                }
            case EditorPage.Items:
                {
                    ItemDataEditor data = new ItemDataEditor();
                    data.ItemID = "Item" + Tools.GetTimeStamp();
                    dpd_ItemType.value = -1;

                    LoadItemsData(data);
                    break;
                }
        }
    }
    void DeleteCurContent()
    {
        switch (curDic)
        {
            default:
                {
                    break;
                }
            case EditorPage.Components:
                {
                    var path = Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/" + curEditComponent.ComponentID + ".json";
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    AddNewContent();

                    break;
                }
            case EditorPage.Entities:
                {
                    var path = Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/" + curSelectedEntity.EntityID + ".json";
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    AddNewContent();
                    break;
                }
            case EditorPage.Items:
                {
                    var path = Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/" + curSelectedItem.ItemID + ".json";
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    AddNewContent();
                    break;
                }
        }
    }
    #endregion
    #region Components
    void LoadComponentData(ComponentData data)
    {
        foreach (var item in compFunctionsItems)
        {
            Destroy(item.gameObject);
        }
        compFunctionsItems.Clear();

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
        ipt_ComponentDescription.text = curEditComponent.ComponentDescription;
        tog_isCompFatal.isOn = curEditComponent.isFatalComponent;
        ipt_ComponentProductor.text = curEditComponent.ComponentProductor;

        dpd_CompFuncType.value = (int)curEditComponent.thisCompType;
        dpd_CompFuncType.captionText.text = curEditComponent.thisCompType.ToString();

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

        CancelFunctionEdit();
    }
    void SaveComponentsData()
    {
        ComponentData newComponentData = new ComponentData();
        newComponentData.ComponentID = ipt_CompID.text;
        newComponentData.ComponentName = ipt_CompName.text;
        newComponentData.ComponentEndurance = float.Parse(ipt_CompEndurance.text);
        newComponentData.ComponentInternalBattery = float.Parse(ipt_CompEnergy.text);
        newComponentData.ComponentDefense = float.Parse(ipt_CompDefense.text);
        newComponentData.ComponentDescription = ipt_ComponentDescription.text;
        newComponentData.isFatalComponent = tog_isCompFatal.isOn;
        newComponentData.ComponentProductor = ipt_ComponentProductor.text;
        newComponentData.thisCompType = (CompType)dpd_CompFuncType.value;
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

        foreach (var page in functionPages)
        {
            page.Value.alpha = 0;
            page.Value.blocksRaycasts = false;
            page.Value.interactable = false;
        }
        if(functionPages.ContainsKey(type))
        {
            functionPages[type].alpha = 1;
            functionPages[type].blocksRaycasts = true;
            functionPages[type].interactable = true;
        }

        ipt_FunctionValue.interactable = true;

        switch ((ComponentFunctionType)dpd_CompType.value)
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

                    txt_FunctionValueDesc.text = "移动力";

                    break;
                }
            case ComponentFunctionType.Weapon:
                {
                    txt_FunctionValueDesc.text = "伤害";

                    dpd_WeapenBallisticType.ClearOptions();
                    foreach (var item in Enum.GetNames(typeof(CompWeapon.WeaponAttackType)))
                    {
                        dpd_WeapenBallisticType.options.Add(new Dropdown.OptionData() { text = item });
                    }

                    break;
                }
            case ComponentFunctionType.Construct:
                {
                    txt_FunctionValueDesc.text = "建造时间";
                    break;
                }
            case ComponentFunctionType.Build:
                {
                    txt_FunctionValueDesc.text = "建造时间";
                    break;
                }
            case ComponentFunctionType.Harvest:
                {
                    txt_FunctionValueDesc.text = "采集范围";
                    break;
                }
            case ComponentFunctionType.Generator:
                {
                    txt_FunctionValueDesc.text = "能量回复";
                    break;
                }
            case ComponentFunctionType.PowerDispatcher:
                {
                    txt_FunctionValueDesc.text = "供能范围";
                    break;
                }
            case ComponentFunctionType.Storage:
                {
                    txt_FunctionValueDesc.text = "存储上限";
                    break;
                }
            case ComponentFunctionType.Resource:
                {
                    txt_FunctionValueDesc.text = "资源获取";

                    dpd_RespurceType.ClearOptions();
                    foreach (var item in Enum.GetNames(typeof(BaseResource.ResourceType)))
                    {
                        dpd_RespurceType.options.Add(new Dropdown.OptionData() { text = item });
                    }
                    break;
                }
        }
    }
    public void LoadCompFunctionDetail(CompFunctionsItem function)
    {
        foreach (var page in functionPages)
        {
            page.Value.alpha = 0;
            page.Value.interactable = false;
            page.Value.blocksRaycasts = false;
        }

        OnComponentTypeChanged((int)function.GetThisFunction().functionType);
        dpd_CompType.value = (int)function.GetThisFunction().functionType;
        dpd_CompType.captionText.text = function.GetThisFunction().functionType.ToString();

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

            selectedIconPath = func.functionIconPath;
            selectedIconIndex = func.functionIconIndex;
        }
        ipt_ApplyTimeInterval.text = func.functionApplyTimeInterval.ToString();
        ipt_FunctionValue.text = func.functionValue.ToString();
        ipt_FunctionConsume.text = func.functionConsume.ToString();
        tog_Auto.isOn = func.canBeAuto;
        ipt_FunctionDesc.text = func.functionDescription;

        foreach (var item in KeyValuePairItems)
        {
            Destroy(item.gameObject);
        }
        KeyValuePairItems.Clear();

        switch ((ComponentFunctionType)dpd_CompType.value)
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

                    txt_FunctionValueDesc.text = "移动力";

                    dpd_MoveType.value = func.functionIntVal[0];
                    dpd_MoveStyle.value = func.functionIntVal[1];
                    break;
                }
            case ComponentFunctionType.Weapon:
                {
                    txt_FunctionValueDesc.text = "伤害";

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
            case ComponentFunctionType.Construct:
                {
                    txt_FunctionValueDesc.text = "建造时间";

                    ipt_ConstructItemID.text = func.functionStringVal[0].ToString();
                    ipt_FunctionValue.text = func.functionFloatVal[0].ToString();
                    for(int i = 1;i<func.functionStringVal.Length;i++)
                    {
                        var pairItem = Instantiate(stringValuePairItem);
                        pairItem.gameObject.SetActive(true);
                        pairItem.transform.SetParent(tsf_constructorKeyValuePairContainer);
                        pairItem.InitThis(StringIndexType.Entity, func.functionStringVal[i], func.functionFloatVal[i], this);
                        KeyValuePairItems.Add(pairItem);
                    }

                    StartCoroutine(RearrangePair(tsf_constructorKeyValuePairContainer));
                    break;
                }
            case ComponentFunctionType.Build:
                {
                    txt_FunctionValueDesc.text = "建造时间";

                    ipt_BuildItemID.text = func.functionStringVal[0].ToString();
                    ipt_FunctionValue.text = func.functionFloatVal[0].ToString();
                    ipt_BuildRange.text = func.functionValue.ToString();
                    for (int i = 1; i < func.functionStringVal.Length; i++)
                    {
                        var pairItem = Instantiate(stringValuePairItem);
                        pairItem.gameObject.SetActive(true);
                        pairItem.transform.SetParent(tsf_builderKeyValuePairContainer);
                        pairItem.InitThis(StringIndexType.Entity, func.functionStringVal[i], func.functionFloatVal[i], this);
                        KeyValuePairItems.Add(pairItem);
                    }

                    StartCoroutine(RearrangePair(tsf_builderKeyValuePairContainer));
                    break;
                }
            case ComponentFunctionType.Harvest:
                {
                    txt_FunctionValueDesc.text = "采集范围";
                    break;
                }
            case ComponentFunctionType.Generator:
                {
                    txt_FunctionValueDesc.text = "能量回复";
                    break;
                }
            case ComponentFunctionType.PowerDispatcher:
                {
                    txt_FunctionValueDesc.text = "供能范围";
                    ipt_MaxPowerDispatchable.text = func.functionFloatVal[0].ToString();
                    break;
                }
            case ComponentFunctionType.Storage:
                {
                    txt_FunctionValueDesc.text = "存储上限";
                    break;
                }
            case ComponentFunctionType.Resource:
                {
                    txt_FunctionValueDesc.text = "资源获取";

                    dpd_RespurceType.captionText.text = ((BaseResource.ResourceType)func.functionIntVal[0]).ToString();
                    dpd_RespurceType.value = func.functionIntVal[0];
                    break;
                }
        }

        btn_ConfirmFunctionEdit.interactable = true;
        btn_CancelFunctinEdit.interactable = true;
    }
    void AddFunction()
    {
        CompFunctionDetail function = new CompFunctionDetail();
        function.functionType = ComponentFunctionType.None;

        OnComponentTypeChanged(0);

        var functionItem = Instantiate(compFunctionsItem);
        functionItem.transform.SetParent(tsf_FunctionsContainer.transform);
        functionItem.gameObject.SetActive(true);
        functionItem.InitThis(function);
        img_Icon.sprite = defaultIcon;

        compFunctionsItems.Add(functionItem);

        curSelectedFunction = functionItem;

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

        newFunction.functionIconPath = selectedIconPath;
        newFunction.functionIconIndex = selectedIconIndex;

        newFunction.functionApplyTimeInterval = float.Parse(ipt_ApplyTimeInterval.text);
        newFunction.functionValue = float.Parse(ipt_FunctionValue.text);
        newFunction.functionConsume = float.Parse(ipt_FunctionConsume.text);
        newFunction.canBeAuto = tog_Auto.isOn;
        newFunction.functionDescription = ipt_FunctionDesc.text;
        newFunction.functionType = (ComponentFunctionType)dpd_CompType.value;

        switch ((ComponentFunctionType)dpd_CompType.value)
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
            case ComponentFunctionType.Construct:
                {
                    List<string> strList = new List<string>();
                    List<float> floatList = new List<float>();
                    strList.Add(ipt_ConstructItemID.text);
                    floatList.Add(float.Parse(ipt_FunctionValue.text));

                    foreach (var item in KeyValuePairItems)
                    {
                        var result = item.GetThisValue();
                        strList.Add(result.str);
                        floatList.Add(result.val);
                    }

                    newFunction.functionStringVal = strList.ToArray();
                    newFunction.functionFloatVal = floatList.ToArray();
                    break;
                }
            case ComponentFunctionType.Build:
                {
                    List<string> strList = new List<string>();
                    List<float> floatList = new List<float>();
                    strList.Add(ipt_BuildItemID.text);
                    floatList.Add(float.Parse(ipt_FunctionValue.text));

                    foreach (var item in KeyValuePairItems)
                    {
                        var result = item.GetThisValue();
                        strList.Add(result.str);
                        floatList.Add(result.val);
                    }

                    newFunction.functionStringVal = strList.ToArray();
                    newFunction.functionFloatVal = floatList.ToArray();
                    newFunction.functionValue = float.Parse(ipt_BuildRange.text);
                    break;
                }
            case ComponentFunctionType.Harvest:
                {
                    break;
                }
            case ComponentFunctionType.Generator:
                {
                    break;
                }
            case ComponentFunctionType.PowerDispatcher:
                {
                    List<float> floatList = new List<float>()
                    {
                        float.Parse(ipt_MaxPowerDispatchable.text)
                    };
                    newFunction.functionFloatVal = floatList.ToArray();
                    break;
                }
            case ComponentFunctionType.Storage:
                {
                    break;
                }
            case ComponentFunctionType.Resource:
                {
                    List<int> intList = new List<int>()
                    {
                        dpd_RespurceType.value,
                    };

                    newFunction.functionIntVal = intList.ToArray();
                    break;
                }
        }
        curSelectedFunction.InitThis(newFunction);
    }
    void CancelFunctionEdit()
    {
        OnComponentTypeChanged(0);

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
    #endregion
    #region Entities
    void LoadEntitiesData(EntityData data)
    {
        curSelectedEntity = data;

        foreach (var item in KeyValuePairItems)
        {
            Destroy(item.gameObject);
        }
        KeyValuePairItems.Clear();

        ipt_EneityID.text = data.EntityID;
        ipt_EntityName.text = data.EntityName;
        ipt_EntityIndex.text = data.EntityIndex;
        dpd_EntityType.value = (int)data.entityType;
        dpd_EntityType.captionText.text = data.entityType.ToString();
        ipt_EntityProductor.text = data.EntityProductor;

        if(data.InstalledComponents != null)
        {
            if (data.InstalledComponents.Length > 0)
            {
                foreach (var item in data.InstalledComponents)
                {
                    var pairItem = Instantiate(stringValuePairItem);
                    pairItem.gameObject.SetActive(true);
                    pairItem.transform.SetParent(tsf_PresetComponentItemContainer);
                    pairItem.InitThis(StringIndexType.Components, item, 0, this);
                    KeyValuePairItems.Add(pairItem);
                }

                StartCoroutine(RearrangePair(tsf_PresetComponentItemContainer));
            }
        }
    }
    void SaveEneitiesData()
    {
        EntityData newEntity = new EntityData();
        newEntity.EntityID = ipt_EneityID.text;
        newEntity.EntityName = ipt_EntityName.text;
        newEntity.EntityIndex = ipt_EntityIndex.text;
        newEntity.entityType = (EntityType)dpd_EntityType.value;
        newEntity.EntityProductor = ipt_EntityProductor.text;

        List<string> stringList = new List<string>();
        foreach (var item in KeyValuePairItems)
        {
            stringList.Add(item.GetThisValue().str);
        }
        newEntity.InstalledComponents = stringList.ToArray();

        var json = JsonConvert.SerializeObject(newEntity);

        var dic = Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/";
        File.WriteAllText(dic + newEntity.EntityID + ".json", json);

        LoadData(curDic);
    }
    #endregion
    #region Items
    void LoadItemsData(ItemDataEditor data)
    {
        curSelectedItem = data;

        ipt_ItemID.text = data.ItemID;
        ipt_ItemName.text = data.ItemName;
        ipt_MaxStackCount.text = data.maxStackCount.ToString();
        tog_Consumeable.isOn = data.consumeable;
        dpd_ItemType.value = (int)data.itemType;
        dpd_ItemType.captionText.text = data.itemType.ToString();
        ipt_ItemProductor.text = data.itemProductor;
        img_ItemIcon.sprite = Tools.GetIcon(data.itemIconPath, data.itemIconIndex);

        selectedIconPath = data.itemIconPath;
        selectedIconIndex = data.itemIconIndex;
    }
    void SaveItemsData()
    {
        ItemDataEditor newItem = new ItemDataEditor();
        newItem.ItemID = ipt_ItemID.text;
        newItem.ItemName = ipt_ItemName.text;
        newItem.maxStackCount = int.Parse(ipt_MaxStackCount.text);
        newItem.consumeable = tog_Consumeable.isOn;
        newItem.itemType = (ItemType)dpd_ItemType.value;
        newItem.itemProductor = ipt_ItemProductor.text;
        newItem.itemIconPath = selectedIconPath;
        newItem.itemIconIndex = selectedIconIndex;

        var json = JsonConvert.SerializeObject(newItem);

        var dic = Application.dataPath + "/Resources/ScriptableItems/" + curDic + "/";
        File.WriteAllText(dic + newItem.ItemID + ".json", json);

        LoadData(curDic);
    }
    #endregion
    #region Utilities
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

        if(!string.IsNullOrEmpty(selectedIconPath))
        {
            img_Icon.sprite = Tools.GetIcon(selectedIconPath, selectedIconIndex);
            img_ItemIcon.sprite = Tools.GetIcon(selectedIconPath, selectedIconIndex);
        }
    }
    void OnCancelIcon()
    {
        selectedIconPath = "";
        selectedIconIndex = -1;

        canvas_IconSelector.alpha = 0;
        canvas_IconSelector.blocksRaycasts = false;
        canvas_IconSelector.interactable = false;
    }
    public void OnConfirmSelectIconItem(IconSelectorItem item)
    {
        selectedIconPath = item.iconName;
        selectedIconIndex = item.iconIndex;

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
    public void RemoveStringValuePair(DataEditorStringValuePair pairItem)
    {
        if(KeyValuePairItems.Contains(pairItem))
        {
            Destroy(pairItem.gameObject);
            KeyValuePairItems.Remove(pairItem);
        }
    }
    void AddConstructorStrValuePair()
    {
        AddStrValuePair(tsf_constructorKeyValuePairContainer, StringIndexType.Entity);
    }
    void AddBuilderStrValuePair()
    {
        AddStrValuePair(tsf_builderKeyValuePairContainer, StringIndexType.Entity);
    }
    void AddPresetComponentsStrValuePair()
    {
        AddStrValuePair(tsf_PresetComponentItemContainer, StringIndexType.Components);
    }
    void AddStrValuePair(Transform tsf_Container, StringIndexType type)
    {
        var pairItem = Instantiate(stringValuePairItem);
        pairItem.gameObject.SetActive(true);
        pairItem.InitThis(type, "", 0, this);
        pairItem.transform.SetParent(tsf_Container);
        KeyValuePairItems.Add(pairItem);

        StartCoroutine(RearrangePair(tsf_Container));
    }
    IEnumerator RearrangePair(Transform tsf_Container)
    {
        tsf_Container.gameObject.SetActive(false);
        yield return null;
        tsf_Container.gameObject.SetActive(true);
    }
    #endregion
}
public enum EditorPage
{
    Components,
    Entities,
    Items,
}
public struct ComponentData
{
    public string ComponentID;
    public string ComponentName;
    public float ComponentEndurance;
    public float ComponentInternalBattery;
    public float ComponentDefense;
    public bool isFatalComponent;
    public CompFunctionDetail[] functions;
    public string ComponentDescription;
    public string ComponentProductor;
    public CompType thisCompType;
}
[Serializable]
public struct CompFunctionDetail
{
    public ComponentFunctionType functionType;
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
    Storage,
    Resource,
}
public enum CompType
{
    Function,
    Base,
    AutoController,
    WallConnector,
    Storage,
    Resource,
}
public enum StringIndexType
{
    Item,
    Entity,
    Components,
}
public struct EntityData
{
    public string EntityID;
    public string EntityName;
    public string EntityIndex;
    public EntityType entityType;
    public string EntityProductor;
    public string[] InstalledComponents;
}
public enum EntityType
{
    Unit,
    Construct,
    Resource,
}
public struct ItemDataEditor
{
    public string ItemID;
    public string ItemName;
    public int maxStackCount;
    public bool consumeable;
    public ItemType itemType;
    public string[] itemStringVal;
    public float[] itemFloatVal;
    public int[] itemIntVal;
    public bool[] itemBoolVal;
    public string itemProductor;
    public string itemIconPath;
    public int itemIconIndex;
}
public enum ItemType
{
    Resource,
    AttachableComponent,
    Misc,
}