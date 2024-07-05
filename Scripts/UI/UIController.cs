using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [Space(1)]
    public GameObject obj_HoveredSkill;
    public Text txt_hoveredSkillName;
    public Text txt_hoveredSkillDesc;
    public Text txt_hoveredSkillCost;
    public Image img_hoveredSkillIcon;
    public Text txt_countType;
    [Space(1)]
    public Text txt_organicAmount;
    public Text txt_constructAmount;
    public Text txt_metalAmount;
    public Text txt_energyLevel;
    [Space(1)]
    public UnitSelectMenu unitMenu;
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
        img_hoveredSkillIcon.sprite = info.functionIcon;
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
}
