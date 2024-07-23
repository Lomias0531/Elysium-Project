using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using static BaseObj;
using static UnityEngine.Rendering.DebugUI;

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
    public static Color HexToColor(string hex)
    {
        Color result;
        if(ColorUtility.TryParseHtmlString(hex, out result))
        {
            return result;
        }else
        {
            return Color.white;
        }
    }
    public static List<BaseTile> GetMobileRange(BaseObj thisEntity, BaseObj.MoveType moveType, BaseObj.MoveStyle moveStyle, int mobility)
    {
        List<BaseTile> result = new List<BaseTile>();

        Dictionary<BaseTile, MoveIndicator> openList = new Dictionary<BaseTile, MoveIndicator>();
        Dictionary<BaseTile, MoveIndicator> closeList = new Dictionary<BaseTile, MoveIndicator>();
        int availableMove;
        List<BaseTile> friendlyTile = new List<BaseTile>();
        var originTile = thisEntity.GetTileWhereUnitIs();

        openList.Add(originTile, new MoveIndicator(originTile, mobility + 1));

        int count = 0;

        do
        {
            //��ʱ��رո���ӵ�Ԫ�������
            var tempList = new Dictionary<BaseTile, MoveIndicator>();
            //��ʱ�ӿ��Ÿ��Ƴ���Ԫ�������
            var tempList1 = new Dictionary<BaseTile, MoveIndicator>();

            foreach (var item in openList)
            {
                //�������еĿ��Ÿ񣬽��������Ƴ�����
                tempList.Add(item.Key, item.Value);

                foreach (var adjTile in item.Value.curTile.adjacentTiles)
                {
                    if (!closeList.ContainsKey(adjTile.Value))
                    {
                        count += 1;

                        //������ǰ��Ԫ������ڵ�Ԫ�����õ�Ԫ�񲻴����ڹرո��У���������ƶ������ġ�
                        var cost = (float)adjTile.Value.GetMoveCost(moveType);
                        if (moveStyle == BaseObj.MoveStyle.Jump || moveStyle == BaseObj.MoveStyle.Teleport)
                        {
                            if(cost >=8)
                            {
                                friendlyTile.Add(adjTile.Value);    
                            }
                            cost = 1;
                        }
                        float life = (float)item.Value.MoveLife - cost;
                        if (!adjTile.Value.isAvailable())
                        {
                            //���������ڵ�Ԫ���ϴ��ڵ�λ����Ϊ�з����ƶ���ֱ�ӹ��㣬��Ϊ�ѷ��ɻ�����λ��Ӱ��
                            var tileUnit = adjTile.Value.GetEntitynThisTile();
                            if (tileUnit != null)
                            {
                                if (tileUnit.Faction == thisEntity.Faction)
                                {
                                    //�������Ԫ������ѷ���Ԫ���б���
                                    friendlyTile.Add(adjTile.Value);
                                }
                                else
                                {
                                    if(moveStyle == BaseObj.MoveStyle.Jump || moveStyle == BaseObj.MoveStyle.Teleport)
                                    {
                                        friendlyTile.Add(adjTile.Value);
                                    }else
                                    {
                                        life = 0;
                                    }
                                }
                            }
                        }
                        if (life > 0 && !tempList1.ContainsKey(adjTile.Value))
                            tempList1.Add(adjTile.Value, new MoveIndicator(adjTile.Value, life));
                    }
                }
            }

            //����ʱ�����еĵ�Ԫ����ӽ������б�
            foreach (var item in tempList1)
            {
                if (!openList.ContainsKey(item.Key))
                    openList.Add(item.Key, item.Value);
            }

            //����ʱ�����еĵ�Ԫ��ӿ����б�����ر��б�
            foreach (var item in tempList)
            {
                if (openList.ContainsKey(item.Key))
                {
                    openList.Remove(item.Key);
                    if (!closeList.ContainsKey(item.Key))
                        closeList.Add(item.Key, item.Value);
                }
            }

            availableMove = 0;
            foreach (var tile in openList)
            {
                if (tile.Value.MoveLife > 0)
                {
                    availableMove += 1;
                }
            }
        } while (availableMove > 0);

        //Debug.Log("Steps: " + count);

        foreach (var tile in friendlyTile)
        {
            if (closeList.ContainsKey(tile))
            {
                closeList.Remove(tile);
            }
        }

        foreach (var tile in closeList)
        {
            result.Add(tile.Key);
        }

        return result;
    }
    public static Vector3 GetBezierCurve(Vector3 startPoint, Vector3 destination, float timeDiv, float maxHeightDev = 0.5f, bool isAdjusting = false, Transform transform = null)
    {
        var length = Vector3.Distance(startPoint, destination);
        var midPoint = (startPoint + destination) / 2 + new Vector3(0, length * maxHeightDev, 0);

        Vector3 curPos = Mathf.Pow((1 - timeDiv), 2) * startPoint + 2 * timeDiv * (1 - timeDiv) * midPoint + Mathf.Pow(timeDiv, 2) * destination;

        if(isAdjusting)
        {
            var tangent = startPoint * 2 * (1 - timeDiv) * -1 + midPoint * 2 * ((1 - timeDiv) + timeDiv * -1) + destination * 2 * timeDiv;
            transform.rotation = Quaternion.LookRotation(tangent, transform.up);
        }

        return curPos;
    }
    public static Vector3 GetBezierCurve(Vector3 startPoint, Vector3 destination, Vector3[] midPoints, float timeDiv, bool isAdjusting = false, Transform transform = null)
    {
        List<Vector3> controlPoints = new List<Vector3>();
        controlPoints.Add(startPoint);
        foreach (var item in midPoints)
        {
            controlPoints.Add(item);
        }
        controlPoints.Add(destination);

        int count = controlPoints.Count;
        Vector3 point = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            double binomialCoeff = BinomialCoefficient(count - 1, i);
            double weight = binomialCoeff * Math.Pow(timeDiv, i) * Math.Pow(1 - timeDiv, count - 1 - i);

            point += controlPoints[i] * (float)weight;
        }

        var curPos = point;

        if (isAdjusting)
        {
            Vector3 tangent = Vector3.zero;

            for(int i = 0;i< count - 1;i++)
            {
                var deltaP = controlPoints[i+1] - controlPoints[i];
                int binomialCoeff = BinomialCoefficient(count - 1, i);
                double factor = (count - 1) * binomialCoeff * Math.Pow(timeDiv, i) * Math.Pow(1 - timeDiv, count - 2 - i);

                // ����t�ĵ����� (i - (n-1-i)*t)  
                factor *= (i - (count - 1 - i) * timeDiv);

                tangent += deltaP * (float)factor;
            }

            transform.rotation = Quaternion.LookRotation(tangent, transform.up);
        }

        return curPos;
    }
    private static int BinomialCoefficient(int n, int k)
    {
        if (k > n) return 0;
        if (k == 0 || k == n) return 1;

        int result = 1;
        for (int i = 1; i <= k; i++)
        {
            result *= (n - i + 1);
            result /= i;
        }
        return result;
    }
    class MoveIndicator
    {
        public BaseTile curTile;
        public float MoveLife;

        public MoveIndicator(BaseTile curTile, float moveLife)
        {
            this.curTile = curTile;
            this.MoveLife = moveLife;
        }
    }
    public static Texture2D GetTextureFromSprite(Sprite sprite)
    {
        var targetTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height);
        targetTex.SetPixels(pixels);
        targetTex.Apply();
        return targetTex;
    }
    public static List<BaseTile> GetTileWithinRange(BaseTile origin, int range, IgnoreType ignoreType, string faction = "Elysium")
    {
        List<BaseTile> result = new List<BaseTile>();

        Dictionary<BaseTile, MoveIndicator> openList = new Dictionary<BaseTile, MoveIndicator>();
        Dictionary<BaseTile, MoveIndicator> closeList = new Dictionary<BaseTile, MoveIndicator>();
        int availableMove;
        List<BaseTile> friendlyTile = new List<BaseTile>();

        openList.Add(origin, new MoveIndicator(origin, range + 1));

        int count = 0;

        do
        {
            //��ʱ��رո���ӵ�Ԫ�������
            var tempList = new Dictionary<BaseTile, MoveIndicator>();
            //��ʱ�ӿ��Ÿ��Ƴ���Ԫ�������
            var tempList1 = new Dictionary<BaseTile, MoveIndicator>();

            foreach (var item in openList)
            {
                //�������еĿ��Ÿ񣬽��������Ƴ�����
                tempList.Add(item.Key, item.Value);

                foreach (var adjTile in item.Value.curTile.adjacentTiles)
                {
                    if (!closeList.ContainsKey(adjTile.Value))
                    {
                        count += 1;

                        //������ǰ��Ԫ������ڵ�Ԫ�����õ�Ԫ�񲻴����ڹرո��У���������ƶ������ġ�
                        float life = (float)item.Value.MoveLife - 1;
                        switch(ignoreType)
                        {
                            case IgnoreType.None:
                                {
                                    if (!adjTile.Value.isAvailable())
                                    {
                                        friendlyTile.Add(adjTile.Value);
                                    }
                                    break;
                                }
                            case IgnoreType.All:
                                {
                                    break;
                                }
                            case IgnoreType.IgnoreFriendly:
                                {
                                    var entity = adjTile.Value.GetEntitynThisTile();
                                    if(entity != null)
                                    {
                                        if(entity.Faction != faction)
                                        {
                                            friendlyTile.Add(adjTile.Value);
                                        }
                                    }
                                    break;
                                }
                            case IgnoreType.IgnoreEnemy:
                                {
                                    var entity = adjTile.Value.GetEntitynThisTile();
                                    if (entity != null)
                                    {
                                        if (entity.Faction == faction)
                                        {
                                            friendlyTile.Add(adjTile.Value);
                                        }
                                    }
                                    break;
                                }
                        }
                        if (life > 0 && !tempList1.ContainsKey(adjTile.Value))
                            tempList1.Add(adjTile.Value, new MoveIndicator(adjTile.Value, life));
                    }
                }
            }

            //����ʱ�����еĵ�Ԫ����ӽ������б�
            foreach (var item in tempList1)
            {
                if (!openList.ContainsKey(item.Key))
                    openList.Add(item.Key, item.Value);
            }

            //����ʱ�����еĵ�Ԫ��ӿ����б�����ر��б�
            foreach (var item in tempList)
            {
                if (openList.ContainsKey(item.Key))
                {
                    openList.Remove(item.Key);
                    if (!closeList.ContainsKey(item.Key))
                        closeList.Add(item.Key, item.Value);
                }
            }

            availableMove = 0;
            foreach (var tile in openList)
            {
                if (tile.Value.MoveLife > 0)
                {
                    availableMove += 1;
                }
            }
        } while (availableMove > 0);

        //Debug.Log("Steps: " + count);

        foreach (var tile in friendlyTile)
        {
            if (closeList.ContainsKey(tile))
            {
                closeList.Remove(tile);
            }
        }

        foreach (var tile in closeList)
        {
            result.Add(tile.Key);
        }

        return result;
    }
    public enum IgnoreType
    {
        None,
        All,
        IgnoreEnemy,
        IgnoreFriendly,
    }
    public static BaseTile GetTileViaCoord(Vector3 coord)
    {
        Ray ray = new Ray(coord + new Vector3(0, 1000f, 0), Vector3.down);
        var result = Physics.RaycastAll(ray, Mathf.Infinity);
        if (result.Length > 0)
        {
            foreach (var hit in result)
            {
                var tile = hit.collider.gameObject.GetComponent<BaseTile>();
                if (tile != null)
                {
                    return tile;
                }
            }
        }
        return null;
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
    public const float outerRadius = 0.5f;
    public const float innerRadius = 0.5f * 0.866025404f;
    public const float extendDistanceX = 0.5f * (1f - 0.866025404f);
    public const float extendDistanceZ = 0.866025404f - 0.75f;
    public static Vector3[] corners =
    {
        new Vector3(0f,0f,outerRadius),
        new Vector3(innerRadius,0f,0.5f * outerRadius),
        new Vector3(innerRadius,0f,-0.5f * outerRadius),
        new Vector3(0f,0f,-outerRadius),
        new Vector3(-innerRadius,0f,-0.5f*outerRadius),
        new Vector3(-innerRadius,0f,0.5f*outerRadius),
        new Vector3(0f,0f,outerRadius)
    };
    public static Vector3[] extendCorners =
    {
        new Vector3(extendDistanceX, 0 ,outerRadius + extendDistanceZ),
        new Vector3(innerRadius + extendDistanceX, 0 ,0.5f*outerRadius + extendDistanceZ),
        new Vector3(innerRadius + extendDistanceX * 2f, 0, 0.5f * outerRadius),
        new Vector3(innerRadius + extendDistanceX * 2f, 0, -0.5f * outerRadius),
        new Vector3(innerRadius + extendDistanceX, 0, -0.5f * outerRadius - extendDistanceZ),
        new Vector3(extendDistanceX, 0, -outerRadius - extendDistanceZ),
        new Vector3(-extendDistanceX, 0, -outerRadius - extendDistanceZ),
        new Vector3(-innerRadius - extendDistanceX, 0, -0.5f * outerRadius - extendDistanceZ),
        new Vector3(-innerRadius - extendDistanceX * 2f, 0, -0.5f * outerRadius),
        new Vector3(-innerRadius - extendDistanceX * 2f, 0, 0.5f * outerRadius),
        new Vector3(-innerRadius - extendDistanceX, 0, 0.5f * outerRadius + extendDistanceZ),
        new Vector3(-extendDistanceX, 0, outerRadius + extendDistanceZ),
    };

    public static Queue<BaseTile> UnitFindPath(this BaseObj unit, BaseTile destination, BaseObj.MoveType selectedMoveType,int moveRange, bool isPathingToTarget = false)
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

        int moveStep = 0;
        while (openPathTiles.Count != 0 && moveStep < moveRange)
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
                //��Ѱ·ģʽΪ������Ŀ�����壬�������ڵظ��Ƿ�ΪĿ�ĵ�
                if(isPathingToTarget)
                {
                    if(adjacentTile == destination)
                    {
                        openPathTiles.Add(adjacentTile);
                    }
                }

                // ���Բ������ߵ����ڵظ�
                if (adjacentTile.GetMoveCost(selectedMoveType) > 8 || (!adjacentTile.isAvailable(true, unit) && adjacentTile != destination))
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
                    adjacentTile.h = Tools.GetEstimatedPathCost(adjacentTile.Pos, destination.Pos) * adjacentTile.GetMoveCost(selectedMoveType);
                    openPathTiles.Add(adjacentTile);
                }
                // ���ʹ�õ�ǰ��G�Ƿ���Եõ�һ�����͵�Fֵ��������ԵĻ�����������ֵ��
                else if (adjacentTile.F > g + adjacentTile.h)
                {
                    adjacentTile.g = g;
                }

                calculateCycle += 1;
            }
            moveStep += 1;
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

        if(isPathingToTarget)
        {
            result.Reverse();
            result.Dequeue();
            result.Reverse();
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
    public static BaseObj GetEntitynThisTile(this BaseTile tile)
    {
        foreach (var unit in MapController.Instance.entityDic)
        {
            if(unit.Value.Pos == tile.Pos)
            {
                return unit.Value;
            }
        }
        return null;
    }
    public static float GetMoveCost(this BaseTile tile, BaseObj.MoveType type)
    {
        if (MoveCostForUnits.ContainsKey(tile.terrainType))
        {
            return MoveCostForUnits[tile.terrainType][(int)type];
        }
        return 100;
    }
    public static bool isAvailable(this BaseTile tile, bool ignoreFriendly, BaseObj me)
    {
        BaseObj unit = tile.GetEntitynThisTile();
        if (unit != null)
        {
            if (ignoreFriendly)
            {
                return unit.Faction == me.Faction;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }
    public static bool isAvailable(this BaseTile tile)
    {
        BaseObj unit = tile.GetEntitynThisTile();
        return unit == null;
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
    public static bool CheckIsTileSuitableForUnit(this BaseObj unit, BaseTile tile)
    {
        bool result = false;
        List<BaseObj.MoveType> list = new List<BaseObj.MoveType>();
        if(unit.moveType != null)
        {
            list = unit.moveType.ToList();
        }
        if(list.Count <= 0)
        {
            list.Add(BaseObj.MoveType.Ground);
        }
        foreach (var move in list)
        {
            if (tile.GetMoveCost(move) < 8 && tile.isAvailable())
            {
                result = true;
            }
        }
        return result;
    }
}