using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using UnityEditor.PackageManager.Requests;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public BaseTile hoveredTile;
    [HideInInspector]
    public BaseObj hoveredObject;
    [HideInInspector]
    public BaseObj selectedObject;

    List<BaseTile> moveIndicators = new List<BaseTile>();
    List<BaseTile> attackIndicators = new List<BaseTile>();
    List<BaseTile> interactIndicators = new List<BaseTile>();

    GameObject selectIndicator;
    Tween selectIndicatorMove;
    public Material selectIndicatorMaterial;
    // Start is called before the first frame update
    void Start()
    {
        CreateSelectIndicator();
    }

    // Update is called once per frame
    void Update()
    {
        GetTileUnderMouse();
        GetObjUnderMouse();
        MouseFunctions();

        PaintIndicator();
    }
    void CreateSelectIndicator()
    {
        selectIndicator = new GameObject("SelectIndicator");

        Mesh hexMesh = selectIndicator.AddComponent<MeshFilter>().mesh = new Mesh();
        MeshRenderer renderer = selectIndicator.AddComponent<MeshRenderer>();

        hexMesh.name = "Hex Mesh";
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        for (int i = 0; i < 6; i++)
        {
            AddTriangle(Vector3.zero, Vector3.zero + ToolsUtility.corners[i], Vector3.zero + ToolsUtility.corners[i + 1], vertices, triangles);
        }

        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();

        renderer.material = selectIndicatorMaterial;
    }
    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, List<Vector3> vertices, List<int> triangles)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
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
                        return;
                    }

                    hoveredTile = selectedTile;

                    if (selectIndicatorMove != null)
                    {
                        selectIndicatorMove.Kill();
                    }
                    selectIndicatorMove = selectIndicator.transform.DOMove(hoveredTile.transform.position + new Vector3(0, 0.01f, 0), 0.1f);

                    //if (hoveredTile && hoveredTile != selectedTile)
                    //{
                    //    hoveredTile.MarkTile(BaseTile.TileSelectionType.None);
                    //}
                    //selectedTile.MarkTile(BaseTile.TileSelectionType.Hover);

                    UIController.Instance.DisplayHoveredTileInfo(hoveredTile);

                    return;
                }
            }
        }
    }
    void GetObjUnderMouse()
    {
        hoveredObject = hoveredTile.GetObjInThisTile();
        UIController.Instance.DisplayHoveredUnitInfo(hoveredObject);
        if (hoveredObject != null)
        {
            
        }
    }
    void MouseFunctions()
    {
        if(Input.GetMouseButtonUp(0))
        {
            if(selectedObject == null)
            {
                selectedObject = hoveredObject;

                UIController.Instance.DisplaySelectedUnitInfo(selectedObject);
            }else
            {

            }
        }
        if(Input.GetMouseButtonUp(1))
        {
            if(MapController.Instance.mapTiles.ContainsKey(selectedObject.Pos))
            {
                MapController.Instance.mapTiles[selectedObject.Pos].MarkTile(BaseTile.TileSelectionType.None);
            }

            selectedObject = null;
            UIController.Instance.DisplaySelectedUnitInfo(null);
        }
    }
    void PaintIndicator()
    {
        if(selectedObject != null)
        {
            PaintSelectionIndicator(selectedObject.Pos);
        }
    }
    void PaintSelectionIndicator(Vector3Int pos)
    {
        if(MapController.Instance.mapTiles.ContainsKey(pos))
        {
            MapController.Instance.mapTiles[pos].MarkTile(BaseTile.TileSelectionType.Selected);
        }
    }
}
