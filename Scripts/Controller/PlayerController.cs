using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public BaseTile hoveredTile;
    public BaseObj hoveredObject;
    public BaseObj selectedObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetTileUnderMouse();
        GetObjUnderMouse();
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
                    if (selectedTile == hoveredTile)
                    {
                        hoveredTile = selectedTile;
                        break;
                    }

                    if (hoveredTile && hoveredTile != selectedTile)
                    {
                        hoveredTile.MarkTile(BaseTile.TileSelectionType.None);
                    }
                    selectedTile.MarkTile(BaseTile.TileSelectionType.Hover);
                    hoveredTile = selectedTile;

                    break;
                }
            }
        }
    }
    void GetObjUnderMouse()
    {
        hoveredObject = hoveredTile.GetObjInThisTile();
        if(hoveredObject != null)
        {

        }
    }
    void UnitFunctions()
    {

    }
}
