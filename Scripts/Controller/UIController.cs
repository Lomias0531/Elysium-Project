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
    public GameObject obj_selectedUnit;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if (obj != null)
        {
            obj_selectedUnit.SetActive(true);
        }
        else
        {
            obj_selectedUnit.SetActive(false);
        }

        img_BG.enabled = false;
        img_BG.enabled = true;
    }
}
