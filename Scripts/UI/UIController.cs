using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : Singletion<UIController>
{
    public ProceduralImage img_BG;
    [Space(1)]
    public GameObject obj_hoveredTile;
    public Text txt_hoveredTileName;
    public Text txt_hoveredTilePos;
    public Text txt_hoveredTileStatus;
    [Space(1)]
    public GameObject obj_hoveredUnit;
    public Text txt_hoveredUnitName;
    public Image img_hoveredUnitHP;
    public Image img_hoveredUnitEP;
    public Image img_hoveredUnitProgress;
    public Transform tsf_InventoryItemContainer;
    [Space(1)]
    public GameObject obj_HoveredSkill;
    public Text txt_hoveredSkillName;
    public Text txt_hoveredSkillDesc;
    public Text txt_hoveredSkillCost;
    public Image img_hoveredSkillIcon;
    public Text txt_countType;
    [Space(1)]
    public GameObject obj_MaterialsMenu;
    public Text txt_organicAmount;
    public Text txt_constructAmount;
    public Text txt_metalAmount;
    public Text txt_energyLevel;
    [Space(1)]
    public UnitSelectMenu unitMenu;
    [Space(1)]
    public UnitIndicatorItem unitIndicatorItem;
    public Transform tsf_UnitIndicatorContainer;
    public Dictionary<BaseObj,UnitIndicatorItem> unitIndicatorItems = new Dictionary<BaseObj, UnitIndicatorItem>();
    [Space(1)]
    public GameObject obj_MaintenanceMenu;
    public Transform tsf_ComponentsSlotItemContainer;
    public MaintenanceSlotItem maintenanceSlotItem;
    public List<MaintenanceSlotItem> maintenanceSlotItems = new List<MaintenanceSlotItem>();
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public enum DisplayInfoType
    {
        skill,
        item,
    }
    // Update is called once per frame
    void Update()
    {
        ShowResourceAmount();
    }
    public void DisplayHoveredTileInfo(BaseTile tile)
    {
        if(tile != null)
        {
            obj_hoveredTile.SetActive(true);
            txt_hoveredTileName.text = tile.tileName;
            txt_hoveredTilePos.text = "(" + tile.Pos.x + "," + tile.Pos.y + "," + tile.Pos.z + ")";
        }else
        {
            obj_hoveredTile.SetActive(false);
        }

        img_BG.enabled = false;
        img_BG.enabled = true;
    }
    public void DisplayHoveredUnitInfo(BaseObj obj)
    {
        if(obj != null)
        {
            obj_hoveredUnit.SetActive(true);
            txt_hoveredUnitName.text = obj.objName;

            img_hoveredUnitHP.fillAmount = obj.HPMax == 0 ? 1 : obj.HP / obj.HPMax;
            img_hoveredUnitEP.fillAmount = obj.EPMax == 0 ? 1 : obj.EP / obj.EPMax;

            img_hoveredUnitProgress.fillAmount = 0;
            var build = obj.GetDesiredComponent<CompConstructTemp>();
            if(build != null)
            {
                img_hoveredUnitProgress.fillAmount = build.buildProgress;
            }
            var construct = obj.GetFunctionComponent(ComponentFunctionType.Construct);
            if(construct != null)
            {
                img_hoveredUnitProgress.fillAmount = construct.progressValue;
            }
        }else
        {
            obj_hoveredUnit.SetActive(false);
        }

        img_BG.enabled = false;
        img_BG.enabled = true;
    }
    public void DisplaySelectedUnitInfo(BaseObj obj)
    {
        unitMenu.OnSelectUnit(obj);
    }
    public void ShowResourceAmount()
    {
        txt_organicAmount.text = PlayerDataManager.Instance.OrganicAmount.ToString("F0") + "/" + PlayerDataManager.Instance.OrganicMaxAmount.ToString("F0");
        txt_constructAmount.text = PlayerDataManager.Instance.ConstructMaterialAmount.ToString("F0") + "/" + PlayerDataManager.Instance.ConstructMaterialMaxAmount.ToString("F0");
        txt_metalAmount.text = PlayerDataManager.Instance.MetalAmount.ToString("F0") + "/" + PlayerDataManager.Instance.MetalMaxAmount.ToString("F0");
        txt_energyLevel.text = PlayerDataManager.Instance.EnergyConsumed.ToString("F0") + "/" + PlayerDataManager.Instance.EnergyProduced.ToString("F0");
    }
    public void DisplayHoveredSkillInfo(CompFunctionDetail info, DisplayInfoType type)
    {
        obj_HoveredSkill.SetActive(true);
        txt_hoveredSkillName.text = info.functionName;
        txt_hoveredSkillCost.text = info.functionConsume.ToString();
        txt_hoveredSkillDesc.text = info.functionDescription;
        img_hoveredSkillIcon.sprite = Tools.GetIcon(info.functionIconPath, info.functionIconIndex);
        switch(type)
        {
            default:
                {
                    break;
                }
            case DisplayInfoType.skill:
                {
                    txt_countType.text = "ÏûºÄ£º";
                    break;
                }
            case DisplayInfoType.item:
                {
                    txt_countType.text = "ÊýÁ¿£º";
                    break;
                }
        }
    }
    public void HideHoveredSkillInfo()
    {
        obj_HoveredSkill.SetActive(false);
    }
    public void CreateUnitIndicators()
    {
        //foreach (var item in MapController.Instance.entityDic)
        //{
        //    if(item.Value.Faction != "Resource")
        //    {
        //        var indicator = GameObject.Instantiate(unitIndicatorItem, tsf_UnitIndicatorContainer);
        //        indicator.InitThis(item.Value);
        //        indicator.gameObject.SetActive(true);
        //        unitIndicatorItems.Add(item.Value, indicator);
        //    }
        //}
    }
    public void AddUnitIndicator(BaseObj obj)
    {
        //var indicator = GameObject.Instantiate(unitIndicatorItem, tsf_UnitIndicatorContainer);
        //indicator.InitThis(obj);
        //indicator.gameObject.SetActive(true);
        //unitIndicatorItems.Add(obj, indicator);
    }
    public void RemoveUnitIndicator(BaseObj obj)
    {
        //if(unitIndicatorItems.ContainsKey(obj))
        //{
        //    Destroy(unitIndicatorItems[obj].gameObject);
        //    unitIndicatorItems.Remove(obj);
        //}
    }
    public void InitMaintenanceScene()
    {
        foreach (var item in maintenanceSlotItems)
        {
            Destroy(item.gameObject);
        }
        maintenanceSlotItems.Clear();

        img_BG.gameObject.SetActive(false);
        obj_MaterialsMenu.gameObject.SetActive(false);
        tsf_UnitIndicatorContainer.gameObject.SetActive(false);
        obj_MaintenanceMenu.gameObject.SetActive(true);

        if (PlayerController.Instance.FocusedUnit == null) return;
        foreach (var basement in PlayerController.Instance.FocusedUnit.componentBasements)
        {
            var item = GameObject.Instantiate(maintenanceSlotItem,tsf_ComponentsSlotItemContainer);
            item.gameObject.SetActive(true);
            item.InitThis(basement);
        }
    }
    public void CloseMaintenanceMenu()
    {
        img_BG.gameObject.SetActive(true);
        obj_MaterialsMenu.gameObject.SetActive(true);
        obj_MaintenanceMenu.gameObject.SetActive(false);
    }
}
