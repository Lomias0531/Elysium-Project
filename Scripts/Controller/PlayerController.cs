using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using UnityEditor.PackageManager.Requests;
using static BaseTile;
using System;
using System.Linq;

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

    Dictionary<string, GameObject> rangeIndicators = new Dictionary<string, GameObject>();
    public Material indicatorCenterMat;
    public Material indicatorBorderMat;
    public float borderWidth;
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
    void AddTriangleReversed(Vector3 v1, Vector3 v2, Vector3 v3, List<Vector3> vertices, List<int> triangles)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex);
    }
    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, List<Vector3> vertices, List<int> triangles)
    {
        int vertexIndex = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
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
                    if (selectedTile == null) return;

                    List<BaseTile> tiles = new List<BaseTile>()
                    {
                        selectedTile,
                    };
                    foreach (var tile in selectedTile.adjacentTiles)
                    {
                        tiles.Add(tile.Value);
                    }
                    DrawRangeIndicator(tiles, selectedTile, "HoverIndicator");

                    hoveredTile = selectedTile;

                    if (selectIndicatorMove != null)
                    {
                        selectIndicatorMove.Kill();
                    }
                    selectIndicatorMove = selectIndicator.transform.DOMove(hoveredTile.transform.position + new Vector3(0, 0.01f, 0), 0.1f);

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
    void DrawRangeIndicator(List<BaseTile> tiles, BaseTile originCenter, string rangeName, float layer = 0)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> borderVertices = new List<Vector3>();
        List<int> borderTriangles = new List<int>();

        DestroyRangeIndicator(rangeName);
        DestroyRangeIndicator(rangeName + "Border");

        GameObject indicatorObj = new GameObject(rangeName);
        indicatorObj.transform.position = originCenter.gameObject.transform.position + new Vector3(0, 0.01f + layer * 0.01f, 0);
        GameObject indicatorBorder = new GameObject(rangeName + "Border");
        indicatorBorder.transform.position = originCenter.gameObject.transform.position + new Vector3(0, 0.02f + layer * 0.01f, 0);

        rangeIndicators.Add(rangeName, indicatorObj);
        rangeIndicators.Add(rangeName + "Border", indicatorBorder);

        Mesh hexMesh = indicatorObj.AddComponent<MeshFilter>().mesh = new Mesh();
        MeshRenderer meshRenderer = indicatorObj.AddComponent<MeshRenderer>();
        Mesh borderMesh = indicatorBorder.AddComponent<MeshFilter>().mesh = new Mesh();
        MeshRenderer borderRenderer = indicatorBorder.AddComponent<MeshRenderer>();

        List<List<Vector3>> tempList = new List<List<Vector3>>();

        foreach (var tile in tiles)
        {
            Vector3 relPos = tile.gameObject.transform.localPosition - originCenter.transform.localPosition;
            for (int i = 0; i < 6; i++)
            {
                AddTriangle(relPos, relPos + ToolsUtility.corners[i], relPos + ToolsUtility.corners[i + 1], vertices, triangles);
            }

            foreach (var adjTile in tile.adjacentTiles)
            {
                var index = (int)adjTile.Key;

                var v1 = relPos + ToolsUtility.corners[index];

                var v2 = relPos + ToolsUtility.corners[index + 1];

                var v3 = relPos + ToolsUtility.extendCorners[index * 2] - new Vector3(0, tile.transform.localPosition.y - tile.adjacentTiles[adjTile.Key].gameObject.transform.localPosition.y, 0);

                var v4 = relPos + ToolsUtility.extendCorners[index * 2 + 1] - new Vector3(0, tile.transform.localPosition.y - tile.adjacentTiles[adjTile.Key].gameObject.transform.localPosition.y, 0);

                AddQuad(v1, v2, (v1 + v3) / 2, (v2 + v4) / 2, vertices, triangles);
            }

            for (int i = 0; i < 6; i++)
            {
                var t = i + 1;
                if (t >= 6) t = 0;
                if (tile.adjacentTiles.ContainsKey((HexDirection)i) && tile.adjacentTiles.ContainsKey((HexDirection)t))
                {
                    var c1 = i + 1 >= 6 ? i - 5 : i + 1;
                    var c2 = i + 3 >= 6 ? i - 3 : i + 3;
                    var c3 = i + 5 >= 6 ? i - 1 : i + 5;
                    var v1 = tile.gameObject.transform.localPosition - originCenter.transform.localPosition + ToolsUtility.corners[c1];
                    var v2 = tile.adjacentTiles[(HexDirection)i].gameObject.transform.localPosition - originCenter.transform.localPosition + ToolsUtility.corners[c2];
                    var v3 = tile.adjacentTiles[(HexDirection)t].gameObject.transform.localPosition - originCenter.transform.localPosition + ToolsUtility.corners[c3];

                    List<Vector3> temp1 = new List<Vector3>()
                    {  v1, v2, v3 };

                    AddTriangle(v1, (v1 + v2) / 2, (v1 + v3) / 2, vertices, triangles);

                    if (tiles.Contains(tile.adjacentTiles[(HexDirection)i]) || tiles.Contains(tile.adjacentTiles[(HexDirection)t]))
                    {
                        bool check = true;
                        foreach (var list in tempList)
                        {
                            if(list.Contains(v1) && list.Contains(v2) && list.Contains(v3))
                            {
                                check = false;
                            }
                        }
                        if(check)
                        {
                            AddTriangleReversed((v1 + v2) / 2, (v1 + v3) / 2, (v2 + v3) / 2, vertices, triangles);
                            tempList.Add(temp1);
                        }
                    }
                }
            }

            foreach (var adjTile in tile.adjacentTiles)
            {
                if (!tiles.Contains(adjTile.Value))
                {
                    var index = (int)adjTile.Key;

                    var v1 = relPos + ToolsUtility.corners[index];

                    var v2 = relPos + ToolsUtility.corners[index + 1];

                    var v3 = relPos + ToolsUtility.extendCorners[index * 2] - new Vector3(0, tile.transform.localPosition.y - tile.adjacentTiles[adjTile.Key].gameObject.transform.localPosition.y, 0);
                    v3 = (v1 + v3) / 2f;

                    var v4 = relPos + ToolsUtility.extendCorners[index * 2 + 1] - new Vector3(0, tile.transform.localPosition.y - tile.adjacentTiles[adjTile.Key].gameObject.transform.localPosition.y, 0);
                    v4 = (v2 + v4) / 2f;

                    var widthDev1 = borderWidth / Vector3.Distance(v1, v3);
                    var widthDev2 = borderWidth / Vector3.Distance(v2, v4);

                    AddQuad(v1, v2, Vector3.Lerp(v1, v3, widthDev1), Vector3.Lerp(v2, v4, widthDev2), borderVertices, borderTriangles);
                }
            }
        }

        hexMesh.SetVertices(vertices);
        hexMesh.SetTriangles(triangles, 0);
        hexMesh.RecalculateNormals();
        meshRenderer.material = indicatorCenterMat;

        borderMesh.SetVertices(borderVertices);
        borderMesh.SetTriangles(borderTriangles, 0);
        borderMesh.RecalculateNormals();
        borderRenderer.material = indicatorBorderMat;

    }
    void DestroyRangeIndicator(string rangeName)
    {
        if(rangeIndicators.ContainsKey(rangeName))
        {
            Destroy(rangeIndicators[rangeName].gameObject);
            rangeIndicators.Remove(rangeName);
        }
    }
}
