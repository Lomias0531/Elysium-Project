using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    BaseTile lastSelectedTile;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetTileUnderMouse();
    }
    void GetTileUnderMouse()
    {
        if (MapController.Instance.mapTiles == null) return;

        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hit = Physics.RaycastAll(camRay, Mathf.Infinity);

        if (hit.Length > 0)
        {
            foreach (var item in hit)
            {
                var selectedTile = item.collider.gameObject.GetComponent<BaseTile>();
                if (selectedTile != null)
                {
                    if (selectedTile == lastSelectedTile)
                    {
                        lastSelectedTile = selectedTile;
                        break;
                    }

                    if (lastSelectedTile && lastSelectedTile != selectedTile)
                    {
                        lastSelectedTile.MarkTile(BaseTile.TileSelectionType.None);
                    }
                    selectedTile.MarkTile(BaseTile.TileSelectionType.Hover);
                    lastSelectedTile = selectedTile;

                    //if (lastSelectedTile != null && !(MoveTileIndicator.ContainsKey(lastSelectedTile.pos) || AttackTileIndicator.ContainsKey(lastSelectedTile.pos)))
                    //{
                    //    lastSelectedTile.SwitchTileStatus(TileStatus.None);
                    //}

                    //var unit = selectedTile.GetUnitOnThisTile();
                    //if (unit != null)
                    //{
                    //    if (unit.isExposed[(int)CombatController.Instance.myTeam])
                    //    {
                    //        if (unit.model.Team == CombatController.Instance.myTeam)
                    //        {
                    //            selectedTile.SwitchTileStatus(TileStatus.Mobilable);
                    //        }
                    //        else
                    //        {
                    //            selectedTile.SwitchTileStatus(TileStatus.Attackable);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        selectedTile.SwitchTileStatus(TileStatus.Float);
                    //    }
                    //}
                    //else
                    //{
                    //    selectedTile.SwitchTileStatus(TileStatus.Float);
                    //}

                    //lastSelectedTile = selectedTile;

                    //lastSelectedTile.OnSelected();

                    //var hover = lastSelectedTile.GetUnitOnThisTile();
                    //if (hover != null)
                    //{
                    //    if (hover.isExposed[(int)CombatController.Instance.myTeam])
                    //    {
                    //        hoverdUnit = hover;
                    //    }
                    //}
                    //else
                    //{
                    //    hoverdUnit = null;
                    //}

                    break;
                }
            }
        }
    }
}
