using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Tools
{
    public static Vector3 CalculateMousePosNormal(Camera _camera, Vector3 _delta, float _distance)
    {
        //通过fov计算旋转中心在视锥截面上的高
        var rect_height = 2 * _distance * Mathf.Tan(_camera.fieldOfView / 2 * Mathf.Deg2Rad);
        //计算屏幕高与中心点截面高的比例,这样能计算出鼠标移动的距离对应在旋转点截面上移动的距离
        var screenRate = rect_height / Screen.height;

        var move_normal = screenRate * _delta.x * _camera.transform.right + screenRate * _delta.y * _camera.transform.up;
        move_normal.y = 0;

        return move_normal;
    }

    // <summary> //
    /// 使用曼哈顿算法 。从给定的起始位置到六角地格的目标位置的估计路径成本。
    // </summary>
    /// <param name="startPosition">起始位置。</param>。
    /// <param name="targetPosition">目标位置。</param>
    public static int GetEstimatedPathCost(Vector3Int startPosition, Vector3Int targetPosition)
    {
        return (Mathf.Abs(startPosition.x - targetPosition.x) + Mathf.Abs(startPosition.y - targetPosition.y) + Mathf.Abs(startPosition.z - targetPosition.z)) / 2;
    }

    public static int GetDistance(Vector3Int start, Vector3Int targetPosition)
    {
        return (Mathf.Abs(start.x - targetPosition.x) + Mathf.Abs(start.y - targetPosition.y) + Mathf.Abs(start.z - targetPosition.z)) / 2;
    }
}
public static class ToolsUtility
{
    public static Dictionary<BaseTile.TerrainType, float[]> MoveCostForUnits = new Dictionary<BaseTile.TerrainType, float[]>
    {
        {BaseTile.TerrainType.Void, new float[]{100,100,100,100 } },
        {BaseTile.TerrainType.Barrier, new float[]{100,100,100,100 } },
        {BaseTile.TerrainType.Water, new float[]{100,10,1,1 } },
        {BaseTile.TerrainType.Plain, new float[]{100,1,10,1 } },
        {BaseTile.TerrainType.Rocks, new float[]{100,2,10,1 } },
        {BaseTile.TerrainType.Swamp, new float[]{100,3,2,1 } },
        {BaseTile.TerrainType.Road, new float[]{100,0.5f,10,1 } },
        {BaseTile.TerrainType.Snowfield, new float[]{100,2,10,1 } },
        {BaseTile.TerrainType.DeepWater, new float[]{100,10,1,1 } },
    };

    public static Queue<BaseTile> UnitFindPath(this BaseObj unit, BaseTile destination)
    {
        int calculateCycle = 0;

        var startPoint = unit.GetTileWhereUnitIs();

        List<BaseTile> openPathTiles = new List<BaseTile>();
        List<BaseTile> closedPathTiles = new List<BaseTile>();

        // 准备好起始地格。
        BaseTile currentTile = startPoint;

        currentTile.g = 0;
        currentTile.h = Tools.GetEstimatedPathCost(startPoint.Pos, destination.Pos);

        // 将开始的地格添加到开放列表中。
        openPathTiles.Add(currentTile);

        while (openPathTiles.Count != 0)
        {
            // 对打开的列表进行排序，以获得F值最低的那块地格。
            openPathTiles = openPathTiles.OrderBy(x => x.F).ThenByDescending(x => x.g).ToList();
            currentTile = openPathTiles[0];

            // 将当前地格从开放列表中移除，并将其添加到封闭列表中。
            openPathTiles.Remove(currentTile);
            closedPathTiles.Add(currentTile);

            float g = currentTile.g + 1;

            // 如果在关闭的列表中有一个目标地格，我们就找到了一个路径。
            if (closedPathTiles.Contains(destination))
            {
                break;
            }

            // 调查当前地格的每一块相邻的地格。
            foreach (BaseTile adjacentTile in currentTile.adjacentTiles.Values)
            {
                // 忽略不能行走的相邻地格。
                if (adjacentTile.GetMoveCost(unit) > 8 || (!adjacentTile.isAvailable(true, unit) && adjacentTile != destination))
                {
                    continue;
                }

                // 忽略已经在关闭列表中的地格。
                if (closedPathTiles.Contains(adjacentTile))
                {
                    continue;
                }

                // 如果它不在开放列表中--添加它并计算G和H。
                if (!(openPathTiles.Contains(adjacentTile)))
                {
                    adjacentTile.g = g;
                    adjacentTile.h = Tools.GetEstimatedPathCost(adjacentTile.Pos, destination.Pos) * adjacentTile.GetMoveCost(unit);
                    openPathTiles.Add(adjacentTile);
                }
                // 检查使用当前的G是否可以得到一个更低的F值，如果可以的话，更新它的值。
                else if (adjacentTile.F > g + adjacentTile.h)
                {
                    adjacentTile.g = g;
                }

                calculateCycle += 1;
            }
        }

        List<BaseTile> finalPathTiles = new List<BaseTile>();

        //回溯--设置最终路径。
        if (closedPathTiles.Contains(destination))
        {
            currentTile = destination;
            finalPathTiles.Add(currentTile);

            for (int i = Mathf.CeilToInt(destination.g - 1); i >= 0; i--)
            {
                currentTile = closedPathTiles.Find(x => x.g == i && currentTile.adjacentTiles.ContainsValue(x));
                finalPathTiles.Add(currentTile);
            }

            finalPathTiles.Reverse();
        }

        Queue<BaseTile> result = new Queue<BaseTile>();
        foreach (var item in finalPathTiles)
        {
            result.Enqueue(item);
        }

        //Debug.Log(calculateCycle);

        return result;
    }
    public static BaseTile GetTileWhereUnitIs(this BaseObj unit)
    {
        foreach (var tile in MapController.Instance.mapTiles)
        {
            if (tile.Value.Pos.x == unit.Pos.x && tile.Value.Pos.y == unit.Pos.y)
            {
                return tile.Value;
            }
        }
        return null;
    }
    public static float GetMoveCost(this BaseTile tile, BaseObj unit)
    {
        if (MoveCostForUnits.ContainsKey(tile.terrainType))
        {
            if ((int)unit.moveType < 5)
            {
                return MoveCostForUnits[tile.terrainType][(int)unit.moveType];
            }
        }
        return 10;
    }
    public static bool isAvailable(this BaseTile tile, bool ignoreFriendly, BaseObj me)
    {
        //BaseObj unit = tile.GetUnitOnThisTile();
        //if (unit != null)
        //{
        //    if (ignoreFriendly)
        //    {
        //        return unit.model.Team == me.model.Team;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        //else
        //{
        //    return true;
        //}
        return true;
    }
    public static BaseObj GetObjInThisTile(this BaseTile tile)
    {
        if (tile == null) return null;
        foreach (var ent in MapController.Instance.entityDic)
        {
            if(ent.Value.Pos == tile.Pos)
            {
                return ent.Value;
            }
        }
        return null;
    }
}