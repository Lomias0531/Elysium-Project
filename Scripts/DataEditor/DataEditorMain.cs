using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    List<string> curEditNames = new List<string>();
    [Space(5)]
    [Header("Components")]
    public InputField ipt_CompID;
    public InputField ipt_CompName;
    public InputField ipt_CompEndurance;
    public InputField ipt_CompEnergy;
    public Dropdown dpd_CompType;
    public Transform tsf_FunctionsContainer;
    public CompFunctionsItem compFunctionsItem;
    public Button btn_AddFunction;
    public Button btn_RemoveFunction;
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
        dpd_SelectItem.onValueChanged.AddListener(LoadSelectedIndex);
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
        var dicPath = Application.dataPath + "/Resources/ScriptableItems/" + type + "/";
        var folderInfo = new DirectoryInfo(dicPath).GetFiles("*").ToList();
        var files = folderInfo.Select(x => x.Name).ToList();
        curDic = type;
        curEditNames = files;
        InputSearchFilter("");
        LoadSelectedIndex(0);
    }
    void InputSearchFilter(string name)
    {
        dpd_SelectItem.ClearOptions();
        List<string> result = curEditNames.FindAll(x=>x.Contains(name)).ToList();
        dpd_SelectItem.AddOptions(result);
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
                    SO_ComponentData data = DataController.Instance.GetComponentData(dpd_SelectItem.options[index].text);
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
}
