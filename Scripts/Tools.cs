using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Tools
{
    public static Vector3 CalculateMousePosNormal(Camera _camera, Vector3 _delta, float _distance)
    {
        //ͨ��fov������ת��������׶�����ϵĸ�
        var rect_height = 2 * _distance * Mathf.Tan(_camera.fieldOfView / 2 * Mathf.Deg2Rad);
        //������Ļ�������ĵ����ߵı���,�����ܼ��������ƶ��ľ����Ӧ����ת��������ƶ��ľ���
        var screenRate = rect_height / Screen.height;

        var move_normal = screenRate * _delta.x * _camera.transform.right + screenRate * _delta.y * _camera.transform.up;
        move_normal.y = 0;

        return move_normal;
    }

    // <summary> //
    /// ʹ���������㷨 ���Ӹ�������ʼλ�õ����ǵظ��Ŀ��λ�õĹ���·���ɱ���
    // </summary>
    /// <param name="startPosition">��ʼλ�á�</param>��
    /// <param name="targetPosition">Ŀ��λ�á�</param>
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

        // ׼������ʼ�ظ�
        BaseTile currentTile = startPoint;

        currentTile.g = 0;
        currentTile.h = Tools.GetEstimatedPathCost(startPoint.Pos, destination.Pos);

        // ����ʼ�ĵظ���ӵ������б��С�
        openPathTiles.Add(currentTile);

        while (openPathTiles.Count != 0)
        {
            // �Դ򿪵��б���������Ի��Fֵ��͵��ǿ�ظ�
            openPathTiles = openPathTiles.OrderBy(x => x.F).ThenByDescending(x => x.g).ToList();
            currentTile = openPathTiles[0];

            // ����ǰ�ظ�ӿ����б����Ƴ�����������ӵ�����б��С�
            openPathTiles.Remove(currentTile);
            closedPathTiles.Add(currentTile);

            float g = currentTile.g + 1;

            // ����ڹرյ��б�����һ��Ŀ��ظ����Ǿ��ҵ���һ��·����
            if (closedPathTiles.Contains(destination))
            {
                break;
            }

            // ���鵱ǰ�ظ��ÿһ�����ڵĵظ�
            foreach (BaseTile adjacentTile in currentTile.adjacentTiles.Values)
            {
                // ���Բ������ߵ����ڵظ�
                if (adjacentTile.GetMoveCost(unit) > 8 || (!adjacentTile.isAvailable(true, unit) && adjacentTile != destination))
                {
                    continue;
                }

                // �����Ѿ��ڹر��б��еĵظ�
                if (closedPathTiles.Contains(adjacentTile))
                {
                    continue;
                }

                // ��������ڿ����б���--�����������G��H��
                if (!(openPathTiles.Contains(adjacentTile)))
                {
                    adjacentTile.g = g;
                    adjacentTile.h = Tools.GetEstimatedPathCost(adjacentTile.Pos, destination.Pos) * adjacentTile.GetMoveCost(unit);
                    openPathTiles.Add(adjacentTile);
                }
                // ���ʹ�õ�ǰ��G�Ƿ���Եõ�һ�����͵�Fֵ��������ԵĻ�����������ֵ��
                else if (adjacentTile.F > g + adjacentTile.h)
                {
                    adjacentTile.g = g;
                }

                calculateCycle += 1;
            }
        }

        List<BaseTile> finalPathTiles = new List<BaseTile>();

        //����--��������·����
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