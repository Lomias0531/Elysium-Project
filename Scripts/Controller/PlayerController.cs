using System.Collections.Generic;
using UnityEngine;
using static BaseTile;
using System.Linq;
using UnityEngine.EventSystems;
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
        GetKeyboardInteractions();

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

                    hoveredTile = selectedTile;

                    if (obj_Build != null && buildIndicator.Contains(selectedTile) && selectedTile.isAvailable())
                    {
                        obj_Build.Pos = selectedTile.Pos;
                        obj_Build.transform.position = selectedTile.transform.position;

                        GetPowerGridRange();
                    }

                    if (attackRangeIndicators.Contains(selectedTile))
                    {
                        GetAOERange();
                    }

                    UIController.Instance.DisplayHoveredTileInfo(hoveredTile);
                    
                    return;
                }
            }
        }
    }
    void GetObjUnderMouse()
    {
        if (hoveredTile == null) return;
        hoveredObject = hoveredTile.GetEntitynThisTile();
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
                        selectedObject.curSelectedComp.OnTriggerFunction(ComponentFunctionType.Mobile, hoveredTile);

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
                                        var resource = res.GetFunctionComponent(ComponentFunctionType.Resource);

                                        resource.OnTriggerFunction( ComponentFunctionType.Resource, selectedObject);
                                    }

                                    break;
                                }
                            case 1:
                                {
                                    break;
                                }
                            case 2:
                                {
                                    var storage = res.GetFunctionComponent(ComponentFunctionType.Storage);
                                    var ees = selectedObject.GetFunctionComponent(ComponentFunctionType.Storage);
                                    var item = ees.thisObj.inventory[selectedObject.curSelectedFunction.functionIntVal[1]];
                                    //ees.TransferItem(storage, item);
                                    ees.OnTriggerFunction(ComponentFunctionType.Storage, storage, item);
                                    break;
                                }
                        }

                        CancelAllOperations();
                    }
                    if(buildIndicator.Contains(hoveredTile))
                    {
                        selectedObject.curSelectedComp.FunctionTriggered(selectedObject.curSelectedFunction);

                        var newConstruct = GameObject.Instantiate(obj_Build, MapController.Instance.entityContainer);
                        newConstruct.transform.eulerAngles = obj_Build.transform.eulerAngles;
                        newConstruct.Faction = "Elysium";
                        newConstruct.InitThis();
                        MapController.Instance.RegisterObject(newConstruct);
                        newConstruct.Pos = hoveredTile.Pos;
                        newConstruct.transform.position = hoveredTile.gameObject.transform.position;
                        newConstruct.curTile = hoveredTile;
                        hoveredTile.curObj = newConstruct;

                        var compBuild = newConstruct.GetDesiredComponent<CompConstructTemp>();
                        compBuild.InitConstruct();

                        CancelAllOperations();
                    }
                    if(attackRangeIndicators.Contains(hoveredTile))
                    {
                        var targetUnit = hoveredTile.GetEntitynThisTile();
                        if(targetUnit != null)
                        {
                            selectedObject.curSelectedComp.FunctionTriggered(selectedObject.curSelectedFunction);
                            selectedObject.curSelectedComp.OnTriggerFunction(ComponentFunctionType.Weapon, targetUnit);
                        }
                    }

                    EntityFinishedAction();
                }
            }
        }
        if(Input.GetMouseButtonUp(1))
        {
            if(RMBPressedTime < 0.2f)
                CancelAllOperations();
        }
    }
    void GetKeyboardInteractions()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(obj_Build != null)
            {
                obj_Build.gameObject.transform.Rotate(new Vector3(0, 60f, 0));
            }
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

        GameObject indicatorObj;
        GameObject indicatorBorder;
        if (rangeIndicators.ContainsKey(rangeName))
        {
            indicatorObj = rangeIndicators[rangeName];
            indicatorBorder = rangeIndicators[rangeName + "Border"];
        }else
        {
            indicatorObj = new GameObject(rangeName);
            indicatorBorder = new GameObject(rangeName + "Border");

            rangeIndicators.Add(rangeName, indicatorObj);
            rangeIndicators.Add(rangeName + "Border", indicatorBorder);
        }

        indicatorObj.SetActive(true);
        indicatorBorder.SetActive(true);

        indicatorObj.transform.position = originCenter.gameObject.transform.position + new Vector3(0, 0.01f + layer * 0.01f, 0);
        indicatorBorder.transform.position = originCenter.gameObject.transform.position + new Vector3(0, 0.02f + layer * 0.01f, 0);
        indicatorBorder.transform.SetParent(indicatorObj.transform);

        var filter = indicatorObj.GetComponent<MeshFilter>();
        if(filter == null)
        {
            filter = indicatorObj.AddComponent<MeshFilter>();
        }
        var meshRenderer = indicatorObj.GetComponent<MeshRenderer>();
        if(meshRenderer == null) meshRenderer = indicatorObj.AddComponent<MeshRenderer>();

        Mesh hexMesh = filter.mesh = new Mesh();

        var borderFilter = indicatorBorder.GetComponent<MeshFilter>();
        if (borderFilter == null)
        {
            borderFilter = indicatorBorder.AddComponent<MeshFilter>();
        }
        var borderRenderer = indicatorBorder.GetComponent<MeshRenderer>();
        if(borderRenderer == null) borderRenderer = indicatorBorder.AddComponent<MeshRenderer>();
        Mesh borderMesh = borderFilter.mesh = new Mesh();

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
            //Destroy(rangeIndicators[rangeName].gameObject);
            //rangeIndicators.Remove(rangeName);
            rangeIndicators[rangeName].gameObject.SetActive(false);
        }
    }

    public void GetMoveRange(List<BaseTile> tiles)
    {
        moveIndicators = tiles;
        DrawRangeIndicator(moveIndicators, selectedObject.GetTileWhereUnitIs(), "MoveIndicator", col_Move, 2f);
    }
    public void GetInteractRange(ComponentFunctionType interactType)
    {
        switch (interactType)
        {
            default:
                {
                    break;
                }
            case ComponentFunctionType.Harvest:
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
            case ComponentFunctionType.Storage:
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
        List<BaseTile> tempGrid = new List<BaseTile>();
        foreach (var construct in PlayerDataManager.Instance.myConstructions)
        {
            var generator = construct.GetFunctionComponent(ComponentFunctionType.PowerDispatcher);
            if(generator != null)
            {
                var radRange = 0f;
                foreach (var func in generator.thisCompData.functions)
                {
                    if(func.functionType == ComponentFunctionType.PowerDispatcher)
                    {
                        radRange = func.functionFloatVal[0];
                    }
                }
                if (radRange <= 0f) return;
                var constructTemp = construct.GetDesiredComponent<CompConstructTemp>();
                if(!construct.isUniderConstruction)
                {
                    var gridList = Tools.GetTileWithinRange(construct.GetTileWhereUnitIs(), (int)radRange, Tools.IgnoreType.All);
                    foreach (var tile in gridList)
                    {
                        if (!powerGridIndicator.Contains(tile))
                        {
                            powerGridIndicator.Add(tile);
                        }
                    }
                }else
                {
                    if(constructTemp.buildProgress > 0)
                    {
                        var gridList = Tools.GetTileWithinRange(construct.GetTileWhereUnitIs(), (int)radRange, Tools.IgnoreType.All);
                        foreach (var tile in gridList)
                        {
                            if (!tempGrid.Contains(tile))
                            {
                                tempGrid.Add(tile);
                            }
                        }
                    }
                    else
                    {
                        var gridList = Tools.GetTileWithinRange(hoveredTile, (int)radRange, Tools.IgnoreType.All);
                        foreach (var tile in gridList)
                        {
                            if (!tempGrid.Contains(tile))
                            {
                                tempGrid.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        Color tempGridColor = col_PowerGrid;
        tempGridColor.a /= 2;
        DrawRangeIndicator(powerGridIndicator, MapController.Instance.mapTiles.FirstOrDefault().Value, "PowerGridIndicator", col_PowerGrid, 2f);
        DrawRangeIndicator(tempGrid, MapController.Instance.mapTiles.FirstOrDefault().Value, "TempPowerGridIndicator", col_PowerGrid, 3f);
    }
    public void GetBuildRange()
    {
        buildIndicator.Clear();
        obj_Build = null;

        var builder = selectedObject.GetFunctionComponent(ComponentFunctionType.Build);
        if(builder != null)
            buildIndicator = Tools.GetTileWithinRange(selectedObject.GetTileWhereUnitIs(), (int)selectedObject.curSelectedFunction.functionValue, Tools.IgnoreType.All);
        List<BaseTile> tilesToRemove = new List<BaseTile>();    
        foreach (var tile in buildIndicator)
        {
            if(tile.terrainType != TerrainType.Plain && tile.terrainType != TerrainType.Rocks)
            {
                tilesToRemove.Add(tile);
            }
            if(!tile.isAvailable())
            {
                tilesToRemove.Add(tile);
            }
        }
        foreach (var item in tilesToRemove)
        {
            buildIndicator.Remove(item);
        }

        DrawRangeIndicator(buildIndicator, MapController.Instance.mapTiles.FirstOrDefault().Value, "BuildIndicator", col_Build, 2f);

        //var obj = DataController.Instance.GetEntityViaID(selectedObject.curSelectedFunction.functionStringVal[0]);
        var obj = DataController.Instance.GetEntityData(selectedObject.curSelectedFunction.functionStringVal[0]);
        if (obj != null)
        {
            obj_Build = GameObject.Instantiate(obj, MapController.Instance.entityContainer);
            obj_Build.thisEntityData = obj.thisEntityData;
            obj_Build.Faction = "Elysium";
            obj_Build.InitThis();
            var temp = obj_Build.AddComponent<CompConstructTemp>();
            obj_Build.Components.Add(temp);
            temp.thisObj = obj_Build;
            temp.buildTime = selectedObject.curSelectedFunction.functionFloatVal[0];
            temp.SimBuild();
            MapController.Instance.RegisterObject(obj_Build);
        }
    }
    public void GetAttackRange()
    {
        attackRangeIndicators.Clear();

        var attackRangeMax = Tools.GetTileWithinRange(selectedObject.GetTileWhereUnitIs(), selectedObject.curSelectedFunction.functionIntVal[1], Tools.IgnoreType.All);
        var attackRangeMin = Tools.GetTileWithinRange(selectedObject.GetTileWhereUnitIs(), selectedObject.curSelectedFunction.functionIntVal[0], Tools.IgnoreType.All);

        foreach (var item in attackRangeMin)
        {
            if(attackRangeMax.Contains(item))
            {
                attackRangeMax.Remove(item);
            }
        }

        attackRangeIndicators = attackRangeMax;

        DrawRangeIndicator(attackRangeIndicators, selectedObject.GetTileWhereUnitIs(), "AttackRangeIndicator", col_AttackRange, 2f);
    }
    void GetAOERange()
    {
        attackIndicators.Clear();

        attackIndicators = Tools.GetTileWithinRange(hoveredTile, selectedObject.curSelectedFunction.functionIntVal[2], Tools.IgnoreType.All);

        DrawRangeIndicator(attackIndicators, hoveredTile, "AttackIndicator", col_Attack, 3f);
    }
    private void OnApplicationFocus(bool focus)
    {
        isFocus = focus;
    }
}
