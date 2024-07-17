using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using UnityEditor.PackageManager.Requests;
using static BaseTile;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEditor.Rendering.LookDev;
using UnityEditor;
using Unity.VisualScripting;

public class PlayerController : Singletion<PlayerController>
{
    [HideInInspector]
    public BaseTile hoveredTile;
    [HideInInspector]
    public BaseObj hoveredObject;
    [HideInInspector]
    public BaseObj selectedObject;

    public Color col_Select;
    public Color col_Move;
    List<BaseTile> moveIndicators = new List<BaseTile>();
    public Color col_Attack;
    List<BaseTile> attackIndicators = new List<BaseTile>();
    public Color col_Interact;
    List<BaseTile> interactIndicators = new List<BaseTile>();
    public Color col_AttackRange;
    List<BaseTile> attackRangeIndicators = new List<BaseTile>();
    public Color col_VisionRange;
    List<BaseTile> visionRangeIndicators = new List<BaseTile>();
    public Color col_PowerGrid;
    List<BaseTile> powerGridIndicator = new List<BaseTile>();
    public Color col_Build;
    List<BaseTile> buildIndicator = new List<BaseTile>();
    BaseObj obj_Build;

    Dictionary<string, GameObject> rangeIndicators = new Dictionary<string, GameObject>();
    public Material indicatorCenterMat;
    public Material indicatorBorderMat;
    public float borderWidth;
    public bool isFocus;
    public bool isMouseOverUI;

    bool RMBPressed = false;
    float RMBPressedTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFocus)
        {
            return;
        }

        RMBPressed = false;
        if(Input.GetMouseButton(1))
        {
            RMBPressed = true;
        }
        if(RMBPressed)
        {
            RMBPressedTime += Time.deltaTime;
        }

        isMouseOverUI = EventSystem.current.IsPointerOverGameObject();

        GetTileUnderMouse();
        GetObjUnderMouse();
        MouseFunctions();

        PaintIndicator();

        if (Input.GetMouseButtonUp(1))
        {
            RMBPressed = false;
            RMBPressedTime = 0;
        }
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
        if (isMouseOverUI) return;

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
                    DrawRangeIndicator(tiles, selectedTile, "HoverIndicator", Color.white);

                    if(obj_Build != null && buildIndicator.Contains(selectedTile) && selectedTile.isAvailable())
                    {
                        obj_Build.Pos = selectedTile.Pos;
                        obj_Build.transform.position = selectedTile.transform.position;

                        GetPowerGridRange();
                    }

                    hoveredTile = selectedTile;

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
            if (isMouseOverUI) return;
            if(selectedObject == null)
            {
                selectedObject = hoveredObject;

                UIController.Instance.DisplaySelectedUnitInfo(selectedObject);

                if(selectedObject) 
                    selectedObject.OnSelected();

                StartCoroutine(CameraController.Instance.CamFocusOnTarget(selectedObject));
            }else
            {
                if (selectedObject.curSelectedComp == null) return;

                if(hoveredTile != null)
                {
                    if(moveIndicators.Contains(hoveredTile))
                    {
                        selectedObject.curSelectedComp.FunctionTriggered(selectedObject.curSelectedFunction);
                        selectedObject.curSelectedComp.OnTriggerFunction(hoveredTile);

                        CameraController.Instance.ResetViewPoint();
                    }
                    if(interactIndicators.Contains(hoveredTile))
                    {
                        selectedObject.curSelectedComp.FunctionTriggered(selectedObject.curSelectedFunction);

                        var res = hoveredTile.GetEntitynThisTile();
                        switch(selectedObject.curSelectedFunction.functionIntVal[0])
                        {
                            default:
                                {
                                    break;
                                }
                            case 0:
                                {
                                    if (res != null)
                                    {
                                        var resource = res.GetDesiredComponent<CompResource>();

                                        resource.OnTriggerFunction(selectedObject);
                                    }

                                    break;
                                }
                            case 1:
                                {
                                    break;
                                }
                            case 2:
                                {
                                    var storage = res.GetDesiredComponent<CompStorage>();
                                    var ees = selectedObject.GetDesiredComponent<CompStorage>();
                                    var item = ees.inventory[selectedObject.curSelectedFunction.functionIntVal[1]];
                                    //ees.TransferItem(storage, item);
                                    ees.OnTriggerFunction(storage, item);
                                    break;
                                }
                        }
                        EntityFinishedAction();
                    }
                    if(buildIndicator.Contains(hoveredTile))
                    {
                        var newConstruct = GameObject.Instantiate(obj_Build, MapController.Instance.entityContainer);
                        newConstruct.Faction = "Elysium";
                        newConstruct.InitThis();
                        MapController.Instance.RegisterObject(newConstruct);
                        newConstruct.Pos = hoveredTile.Pos;
                        newConstruct.transform.position = hoveredTile.gameObject.transform.position;
                        var compBuild = newConstruct.AddComponent<CompConstructTemp>();
                        newConstruct.components.Add(compBuild);
                        compBuild.thisObj = newConstruct;
                        compBuild.buildTime = selectedObject.curSelectedFunction.functionFloatVal[0];
                        compBuild.InitConstruct();

                        CancelAllOperations();
                    }
                }
            }
        }
        if(Input.GetMouseButtonUp(1))
        {
            if(RMBPressedTime < 0.2f)
                CancelAllOperations();
        }
    }
    public void CancelAllOperations()
    {
        foreach (var item in rangeIndicators)
        {
            Destroy(item.Value.gameObject);
        }
        rangeIndicators.Clear();

        if(selectedObject)
            selectedObject.OnUnselected();

        selectedObject = null;
        UIController.Instance.DisplaySelectedUnitInfo(null);
        StartCoroutine(CameraController.Instance.CamFocusOnTarget(null));
        moveIndicators.Clear();
        attackIndicators.Clear();
        interactIndicators.Clear();
        attackRangeIndicators.Clear();
        visionRangeIndicators.Clear();
        powerGridIndicator.Clear();
        buildIndicator.Clear();
        if(obj_Build != null)
        {
            UIController.Instance.RemoveUnitIndicator(obj_Build);
            MapController.Instance.RemoveObject(obj_Build);
            obj_Build = null;
        }
    }
    public void EntityFinishedAction()
    {
        foreach (var item in rangeIndicators)
        {
            Destroy(item.Value.gameObject);
        }
        rangeIndicators.Clear();

        //selectedObject.curSelectedComp = null;
        //selectedObject.curSelectedFunction = null;
    }
    void PaintIndicator()
    {
        if(selectedObject != null)
        {
            List<BaseTile> tiles = new List<BaseTile>()
            {
                selectedObject.GetTileWhereUnitIs(),
            };
            DrawRangeIndicator(tiles, selectedObject.GetTileWhereUnitIs(), "SelectIndicator", col_Select, 1f);
        }
    }
    void DrawRangeIndicator(List<BaseTile> tiles, BaseTile originCenter,string rangeName, Color rangeColor, float layer = 0)
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
        indicatorBorder.transform.SetParent(indicatorObj.transform);

        rangeIndicators.Add(rangeName, indicatorObj);
        rangeIndicators.Add(rangeName + "Border", indicatorBorder);

        Mesh hexMesh = indicatorObj.AddComponent<MeshFilter>().mesh = new Mesh();
        MeshRenderer meshRenderer = indicatorObj.AddComponent<MeshRenderer>();
        Mesh borderMesh = indicatorBorder.AddComponent<MeshFilter>().mesh = new Mesh();
        MeshRenderer borderRenderer = indicatorBorder.AddComponent<MeshRenderer>();

        List<List<Vector3>> tempList = new List<List<Vector3>>();

        foreach (var tile in tiles)
        {
            //DrawCenter
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

            //DrawBorder
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

                    AddQuad(Vector3.Lerp(v1, v3, 1f - widthDev1), Vector3.Lerp(v2, v4, 1f - widthDev2), v3, v4, borderVertices, borderTriangles);
                }
                var nextAdj = (int)adjTile.Key + 1 >= 6 ? 0 : (int)adjTile.Key + 1;
                var nextDir = (HexDirection)nextAdj;
                if(tile.adjacentTiles.ContainsKey(nextDir) && !tiles.Contains(adjTile.Value))
                {
                    if (!tiles.Contains(tile.adjacentTiles[nextDir]))
                    {
                        var c1 = (int)adjTile.Key + 1 >= 6 ? (int)adjTile.Key - 5 : (int)adjTile.Key + 1;
                        var c2 = (int)adjTile.Key + 3 >= 6 ? (int)adjTile.Key - 3 : (int)adjTile.Key + 3;
                        var c3 = (int)adjTile.Key + 5 >= 6 ? (int)adjTile.Key - 1 : (int)adjTile.Key + 5;

                        var v1 = tile.transform.localPosition - originCenter.transform.localPosition + ToolsUtility.corners[c1];

                        var v2 = adjTile.Value.transform.localPosition - originCenter.transform.localPosition + ToolsUtility.corners[c2];
                        v2 = (v1 + v2) / 2f;

                        var v3 = tile.adjacentTiles[nextDir].transform.localPosition - originCenter.transform.localPosition + ToolsUtility.corners[c3];
                        v3 = (v1 + v3) / 2f;

                        var widthDev1 = borderWidth / Vector3.Distance(v1, v2);
                        var widthDev2 = borderWidth / Vector3.Distance(v1, v3);

                        AddQuad(Vector3.Lerp(v1, v2, 1f - widthDev1), Vector3.Lerp(v1, v3, 1f - widthDev2), v2, v3, borderVertices, borderTriangles);
                    }
                    else
                    {
                        var c1 = (int)adjTile.Key + 1 >= 6 ? (int)adjTile.Key - 5 : (int)adjTile.Key + 1;
                        var c2 = (int)adjTile.Key + 3 >= 6 ? (int)adjTile.Key - 3 : (int)adjTile.Key + 3;
                        var c3 = (int)adjTile.Key + 5 >= 6 ? (int)adjTile.Key - 1 : (int)adjTile.Key + 5;

                        var v1 = tile.transform.localPosition - originCenter.transform.localPosition + ToolsUtility.corners[c1];

                        var v2 = adjTile.Value.transform.localPosition - originCenter.transform.localPosition + ToolsUtility.corners[c2];

                        var v3 = tile.adjacentTiles[nextDir].transform.localPosition - originCenter.transform.localPosition + ToolsUtility.corners[c3];

                        var v4 = (v1 + v2) / 2f;
                        var v5 = (v2 + v3) / 2f;

                        var widthDev1 = borderWidth / Vector3.Distance(v1, v4);
                        var widthDev2 = borderWidth / Vector3.Distance(v3, v5);

                        AddQuad(Vector3.Lerp(v1, v4, 1f - widthDev1), Vector3.Lerp(v3, v5, 1f - widthDev2), v4, v5, borderVertices, borderTriangles);
                    }
                }
            }
        }

        hexMesh.SetVertices(vertices);
        hexMesh.SetTriangles(triangles, 0);
        hexMesh.RecalculateNormals();
        meshRenderer.material = indicatorCenterMat;
        meshRenderer.material.SetColor("_Color", rangeColor);

        borderMesh.SetVertices(borderVertices);
        borderMesh.SetTriangles(borderTriangles, 0);
        borderMesh.RecalculateNormals();
        borderRenderer.material = indicatorBorderMat;
        borderRenderer.material.SetColor("_Color", rangeColor);
    }
    void DestroyRangeIndicator(string rangeName)
    {
        if(rangeIndicators.ContainsKey(rangeName))
        {
            Destroy(rangeIndicators[rangeName].gameObject);
            rangeIndicators.Remove(rangeName);
        }
    }

    public void GetMoveRange(List<BaseTile> tiles)
    {
        moveIndicators = tiles;
        DrawRangeIndicator(moveIndicators, selectedObject.GetTileWhereUnitIs(), "MoveIndicator", col_Move, 2f);
    }
    public void GetInteractRange(BaseComponent.InteractFunction interactType)
    {
        switch (interactType)
        {
            default:
                {
                    break;
                }
            case BaseComponent.InteractFunction.Harvest:
                {
                    interactIndicators.Clear();
                    var curTile = selectedObject.GetTileWhereUnitIs();
                    foreach (var adjTile in curTile.adjacentTiles)
                    {
                        var obj = adjTile.Value.GetEntitynThisTile();
                        if (obj != null)
                        {
                            if(obj.GetDesiredComponent<CompResource>() != null)
                            {
                                interactIndicators.Add(adjTile.Value);
                            }
                        }
                    }
                    break;
                }
            case BaseComponent.InteractFunction.Store:
                {
                    interactIndicators.Clear();
                    var curTile = selectedObject.GetTileWhereUnitIs();
                    foreach (var adjTile in curTile.adjacentTiles)
                    {
                        var obj = adjTile.Value.GetEntitynThisTile();
                        if(obj != null)
                        {
                            if(obj.GetDesiredComponent<CompStorage>() != null)
                            {
                                interactIndicators.Add(adjTile.Value);
                            }
                        }
                    }
                    break;
                }
        }
        if(interactIndicators.Count > 0)
        {
            DrawRangeIndicator(interactIndicators, selectedObject.GetTileWhereUnitIs(), "InteractIndicator", col_Interact, 2f);
        }
    }
    public void GetPowerGridRange()
    {
        powerGridIndicator.Clear();
        foreach (var construct in PlayerDataManager.Instance.myConstructions)
        {
            var generator = construct.GetDesiredComponent<CompPowerDispathcer>();
            if(generator != null)
            {
                var gridList = Tools.GetTileWithinRange(construct.GetTileWhereUnitIs(), generator.powerRadiationRange, Tools.IgnoreType.All);
                foreach (var tile in gridList)
                {
                    if(!powerGridIndicator.Contains(tile))
                    {
                        powerGridIndicator.Add(tile);
                    }
                }
            }
        }

        DrawRangeIndicator(powerGridIndicator, MapController.Instance.mapTiles.FirstOrDefault().Value, "PowerGridIndicator", col_PowerGrid, 2f);
    }
    public void GetBuildRange()
    {
        buildIndicator.Clear();
        obj_Build = null;

        var builder = selectedObject.GetDesiredComponent<CompBuilder>();
        if(builder != null)
            buildIndicator = Tools.GetTileWithinRange(selectedObject.GetTileWhereUnitIs(), builder.buildRange, Tools.IgnoreType.All);

        DrawRangeIndicator(buildIndicator, MapController.Instance.mapTiles.FirstOrDefault().Value, "BuildIndicator", col_Build, 2f);

        var obj = DataController.Instance.GetEntityViaID(selectedObject.curSelectedFunction.functionStringVal[0]);
        if (obj != null)
        {
            obj_Build = GameObject.Instantiate(obj, MapController.Instance.entityContainer);
            obj_Build.Faction = "Elysium";
            obj_Build.InitThis();
            MapController.Instance.RegisterObject(obj_Build);
        }
    }
    private void OnApplicationFocus(bool focus)
    {
        isFocus = focus;
    }
}
