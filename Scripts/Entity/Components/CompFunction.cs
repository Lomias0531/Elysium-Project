using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BaseObj;
using static CompWeapon;
using Random = UnityEngine.Random;

public class CompFunction : BaseComponent
{
    Coroutine attactCoroutine;
    BaseTile attackTile;

    float valueTimeRequired;
    float valueTimeElapsed;
    public bool isFunctionProgressing;
    public float progressValue
    {
        get
        {
            if(valueTimeRequired != 0)
            {
                return valueTimeElapsed / valueTimeRequired;
            }else
            {
                return 0;
            }
        }
    }
    int curSelectedIndex;

    public override void OnApply(int index)
    {
        if (isFunctionProgressing) return;
        switch(thisCompData.functions[index].functionType)
        {
            default:
                {
                    break;
                }
            case ComponentFunctionType.Mobile:
                {
                    var moveType = (BaseObj.MoveType)thisCompData.functions[index].functionIntVal[0];
                    var moveStyle = (BaseObj.MoveStyle)thisCompData.functions[index].functionIntVal[1];
                    var mobility = (int)thisCompData.functions[index].functionValue;

                    PlayerController.Instance.GetMoveRange(Tools.GetMobileRange(thisObj, moveType, moveStyle, mobility));

                    break;
                }
            case ComponentFunctionType.Weapon:
                {
                    PlayerController.Instance.GetAttackRange();
                    break;
                }
            case ComponentFunctionType.Harvest:
                {
                    PlayerController.Instance.GetInteractRange(ComponentFunctionType.Harvest);
                    break;
                }
            case ComponentFunctionType.Construct:
                {
                    if (isFunctionProgressing) return;
                    var storage = thisObj.GetFunctionComponent(ComponentFunctionType.Storage);
                    bool checkResources = true;
                    if (storage != null)
                    {
                        for (int i = 1; i < thisCompData.functions[index].functionStringVal.Length; i++)
                        {
                            if (storage.GetItemCount(thisCompData.functions[index].functionStringVal[i]) < thisCompData.functions[index].functionFloatVal[i])
                            {
                                checkResources = false;
                            }
                        }
                    }
                    else
                    {
                        checkResources = true;
                    }

                    int availableTileCount = 0;
                    var obj = DataController.Instance.GetEntityData(thisCompData.functions[index].functionStringVal[0]);
                    foreach (var adjTile in thisObj.GetTileWhereUnitIs().adjacentTiles)
                    {
                        if (obj.CheckIsTileSuitableForUnit(adjTile.Value))
                        {
                            availableTileCount += 1;
                        }
                    }
                    if (availableTileCount <= 0)
                    {
                        checkResources = false;
                    }

                    if (checkResources)
                    {
                        curSelectedIndex = index;
                        isFunctionProgressing = true;
                        for (int i = 1; i < thisCompData.functions[index].functionStringVal.Length; i++)
                        {
                            ItemData item = new ItemData();
                            item.itemID = thisCompData.functions[index].functionStringVal[i];
                            item.stackCount = (int)thisCompData.functions[index].functionFloatVal[i];

                            storage.RemoveItem(item);
                        }
                        valueTimeElapsed = 0;
                        valueTimeRequired = thisCompData.functions[index].functionFloatVal[0];
                    }
                    break;
                }
            case ComponentFunctionType.Build:
                {
                    if (isFunctionProgressing) return;
                    var storage = thisObj.GetFunctionComponent(ComponentFunctionType.Storage);
                    bool checkResources = true;
                    if (storage != null)
                    {
                        for (int i = 1; i < thisCompData.functions[index].functionStringVal.Length; i++)
                        {
                            if (storage.GetItemCount(thisCompData.functions[index].functionStringVal[i]) < thisCompData.functions[index].functionFloatVal[i])
                            {
                                checkResources = false;
                            }
                        }
                    }
                    else
                    {
                        checkResources = true;
                    }

                    if (checkResources)
                    {
                        PlayerController.Instance.GetBuildRange();
                    }

                    break;
                }
            case ComponentFunctionType.Production:
                {
                    if (isFunctionProgressing) return;

                    var storage = thisObj.GetFunctionComponent(ComponentFunctionType.Storage);
                    bool checkResources = true;
                    if (storage != null)
                    {
                        for (int i = 1; i < thisCompData.functions[index].functionStringVal.Length; i++)
                        {
                            if (storage.GetItemCount(thisCompData.functions[index].functionStringVal[i]) < thisCompData.functions[index].functionFloatVal[i])
                            {
                                checkResources = false;
                            }
                        }
                    }
                    else
                    {
                        checkResources = true;
                    }

                    if(checkResources)
                    {
                        curSelectedIndex = index;
                        isFunctionProgressing = true;
                        for (int i = 1; i < thisCompData.functions[index].functionStringVal.Length; i++)
                        {
                            ItemData item = new ItemData();
                            item.itemID = thisCompData.functions[index].functionStringVal[i];
                            item.stackCount = (int)thisCompData.functions[index].functionFloatVal[i];

                            storage.RemoveItem(item);
                        }
                        valueTimeElapsed = 0;
                        valueTimeRequired = thisCompData.functions[index].functionFloatVal[0];
                    }
                    break;
                }
        }
    }

    public override void OnDestroyThis()
    {
        
    }

    public override void OnTriggerFunction(ComponentFunctionType type, params object[] obj)
    {
        switch (type)
        {
            default:
                {
                    break;
                }
            case ComponentFunctionType.Mobile:
                {
                    if (obj[0] is BaseTile)
                    {
                        StartCoroutine(MoveObject((BaseTile)obj[0]));
                    }
                    break;
                }
            case ComponentFunctionType.Weapon:
                {
                    if (attactCoroutine != null)
                        StopCoroutine(attactCoroutine);

                    if ((BaseTile)obj[0] != null)
                    {
                        attackTile = (BaseTile)obj[0];

                        CommenceAttack(attackTile);
                    }
                    break;
                }
            case ComponentFunctionType.Resource:
                {
                    if (obj[0] is BaseObj)
                    {
                        var storage = ((BaseObj)obj[0]).GetFunctionComponent(ComponentFunctionType.Storage);
                        if (storage != null)
                        {
                            for (int i = 0; i < thisObj.curSelectedFunction.functionStringVal.Length; i++)
                            {
                                ItemData data = new ItemData();
                                data.itemID = thisObj.curSelectedFunction.functionStringVal[i];
                                data.stackCount = thisObj.curSelectedFunction.functionIntVal[i];
                                storage.ReceiveItem(data);
                            }
                        }
                    }
                    break;
                }
            case ComponentFunctionType.Storage:
                {
                    if (obj[0] is CompStorage && obj[1] is ItemData)
                    {
                        CompStorage targetStorage = (CompStorage)obj[0];
                        ItemData transferedItem = (ItemData)obj[1];

                        for (int i = thisObj.inventory.Count - 1; i >= 0; i--)
                        {
                            if (thisObj.inventory[i].itemID == transferedItem.itemID)
                            {
                                if (thisObj.inventory[i].stackCount <= transferedItem.stackCount)
                                {
                                    ItemData dataTemp = new ItemData();
                                    dataTemp.itemID = thisObj.inventory[i].itemID;
                                    dataTemp.stackCount = thisObj.inventory[i].stackCount;

                                    var itemData = targetStorage.ReceiveItem(dataTemp);
                                    transferedItem.stackCount -= thisObj.inventory[i].stackCount;
                                    if (itemData.stackCount <= 0)
                                    {
                                        SetInvCount(i, 0);
                                    }
                                    else
                                    {
                                        SetInvCount(i, itemData.stackCount);
                                    }
                                }
                                else
                                {
                                    var itemCount = transferedItem.stackCount;
                                    var itemData = targetStorage.ReceiveItem(transferedItem);
                                    var itemTransfered = itemCount - itemData.stackCount;

                                    transferedItem.stackCount -= itemTransfered;
                                    SetInvCount(i, thisObj.inventory[i].stackCount - itemTransfered);
                                }
                            }
                            if (thisObj.inventory[i].stackCount <= 0)
                            {
                                thisObj.inventory.RemoveAt(i);
                            }
                            if (transferedItem.stackCount <= 0) break;
                        }
                    }
                    break;
                }
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (thisObj.isUniderConstruction) return;

        foreach (var func in thisCompData.functions)
        {
            if(func.functionType == ComponentFunctionType.Generator)
            {
                this.EP += func.functionValue * Time.deltaTime;
            }
            if(func.functionType == ComponentFunctionType.PowerDispatcher)
            {

            }
        }

        if (!isFunctionProgressing) return;
        this.EP -= thisCompData.functions[curSelectedIndex].functionConsume * Time.deltaTime;
        if (this.EP < 0)
        {
            this.EP = 0;
        }
        else
        {
            if (valueTimeElapsed < valueTimeRequired)
            {
                valueTimeElapsed += Time.deltaTime;
            }
            else
            {
                switch (thisCompData.functions[curSelectedIndex].functionType)
                {
                    default:
                        {
                            break;
                        }
                    case ComponentFunctionType.Construct:
                        {
                            StartCoroutine(constructItem());
                            break;
                        }
                    case ComponentFunctionType.Production:
                        {
                            var storage = thisObj.GetFunctionComponent(ComponentFunctionType.Storage);
                            ItemData item = new ItemData();
                            item.itemID = thisCompData.functions[curSelectedIndex].functionStringVal[0];
                            item.stackCount = (int)thisCompData.functions[curSelectedIndex].functionFloatVal[0];

                            storage.ReceiveItem(item);
                            break;
                        }
                }

                isFunctionProgressing = false;
                valueTimeElapsed = 0;
            }
        }
    }
    #region FuncDetail
    #region Mobile
    public IEnumerator MoveObject(BaseTile tile)
    {
        if (tile == null) yield break;
        //isMoving = true;
        var moveQueue = thisObj.UnitFindPath(tile, (MoveType)thisObj.curSelectedFunction.functionIntVal[0], (int)thisObj.curSelectedFunction.functionValue);

        if (moveQueue.Count <= 0)
        {
            moveQueue.Enqueue(tile);
        }
        StartCoroutine(MoveObject(moveQueue));
    }

    public IEnumerator MoveObject(Queue<BaseTile> moveQueue)
    {
        if ((moveQueue == null || moveQueue.Count == 0) && (MoveStyle)thisObj.curSelectedFunction.functionIntVal[1] == MoveStyle.Ordinary)
        {
            var cpu = thisObj.GetDesiredComponent<CompAutoController>();
            if (cpu != null)
            {
                cpu.ReceiveActionException(CompAutoController.UnitActException.IllegalMove);
            }
            yield break;
        }
        var tile = moveQueue.Last();

        switch ((MoveStyle)thisObj.curSelectedFunction.functionIntVal[1])
        {
            default:
                {
                    if (thisObj.animator != null)
                    {
                        thisObj.animator.CrossFadeInFixedTime("Walk", 0.1f);
                        yield return new WaitForSeconds(0.1f);
                    }

                    do
                    {
                        var target = moveQueue.Dequeue();
                        if (target == thisObj.GetTileWhereUnitIs())
                            continue;

                        var facing = Vector3.Angle(new Vector3(0, 0, 1f), target.transform.position - this.transform.position);
                        if (target.transform.position.x < this.transform.position.x) facing *= -1;

                        this.transform.localEulerAngles = new Vector3(0, facing, 0);

                        float moveTimeElapsed = 0;
                        var originPos = this.transform.position;

                        bool moveRight = true;
                        do
                        {
                            this.transform.position = Vector3.Lerp(originPos, target.transform.position, moveTimeElapsed / 0.2f);
                            moveTimeElapsed += Time.deltaTime;
                            if (target.curObj != null)
                            {
                                moveRight = false;
                                moveTimeElapsed = 0f;
                                break;
                            }
                            yield return null;
                        } while (moveTimeElapsed < 0.2f);

                        if (target.curObj != null)
                        {
                            moveRight = false;
                        }

                        if (!moveRight)
                        {
                            this.transform.position = thisObj.curTile.transform.position;
                            var auto = thisObj.GetDesiredComponent<CompAutoController>();
                            auto.ReceiveActionException(CompAutoController.UnitActException.IllegalMove);
                            break;
                        }
                        else
                        {
                            this.transform.position = target.transform.position;
                            thisObj.Pos = target.Pos;

                            thisObj.curTile.curObj = null;
                            thisObj.curTile = target;
                            target.curObj = thisObj;
                        }
                    } while (moveQueue.Count > 0);

                    break;
                }
            case MoveStyle.Teleport:
                {
                    if (thisObj.animator != null)
                    {
                        thisObj.animator.CrossFadeInFixedTime("Interact", 0.1f);
                        yield return new WaitForSeconds(0.2f);
                        yield return new WaitForSeconds(thisObj.animator.GetCurrentAnimatorStateInfo(0).length);
                    }

                    var facing = Vector3.Angle(new Vector3(0, 0, 1f), tile.transform.position - this.transform.position);
                    if (tile.transform.position.x < this.transform.position.x) facing *= -1;

                    this.transform.localEulerAngles = new Vector3(0, facing, 0);

                    //yield return new WaitForSeconds(0.2f);

                    this.transform.position = tile.transform.position;
                    thisObj.Pos = tile.Pos;

                    thisObj.curTile.curObj = null;
                    thisObj.curTile = tile;
                    tile.curObj = thisObj;
                    break;
                }
            case MoveStyle.Jump:
                {
                    if (thisObj.animator != null)
                    {
                        thisObj.animator.CrossFadeInFixedTime("Jump", 0.1f);
                        yield return new WaitForSeconds(0.1f);
                        //animator.speed = animator.GetCurrentAnimatorStateInfo(0).length / 1f;
                    }

                    var facing = Vector3.Angle(new Vector3(0, 0, 1f), tile.transform.position - this.transform.position);
                    if (tile.transform.position.x < this.transform.position.x) facing *= -1;

                    this.transform.localEulerAngles = new Vector3(0, facing, 0);

                    float jumpTileElapsed = 0;
                    do
                    {
                        this.transform.position = Tools.GetBezierCurve(thisObj.GetTileWhereUnitIs().transform.position, tile.transform.position, jumpTileElapsed / 1f, 5f);
                        jumpTileElapsed += Time.deltaTime;

                        yield return null;
                    } while (jumpTileElapsed <= 1f);
                    this.transform.position = tile.transform.position;
                    thisObj.Pos = tile.Pos;

                    thisObj.curTile.curObj = null;
                    thisObj.curTile = tile;
                    tile.curObj = thisObj;

                    break;
                }
        }

        //PlayerController.Instance.EntityFinishedAction();

        if (thisObj.animator != null)
        {
            thisObj.animator.speed = 1f;
            thisObj.animator.CrossFadeInFixedTime("Idle", 0.1f);
        }
        yield return new WaitForSeconds(0.1f);

        //isMoving = false;
    }
    #endregion
    #region Weapon
    public void CommenceAttack(BaseTile targetTile)
    {
        if (targetTile == null)
        {
            var cpu = thisObj.GetDesiredComponent<CompAutoController>();
            if (cpu != null) cpu.ReceiveActionException(CompAutoController.UnitActException.IllegalAttack);
            return;
        }
        attackTile = targetTile;
        if (EP < thisObj.curSelectedFunction.functionConsume) return;

        if (thisObj.tsf_Turret != null)
        {
            var dir = thisObj.gameObject.transform.position - attackTile.gameObject.transform.position;
            dir.y = 0;

            float angle = Vector3.SignedAngle(thisObj.tsf_Turret.forward, dir, Vector3.up);
            float rotateDir = Mathf.Sign(angle);

            float step;

            if (Mathf.Abs(angle) >= thisObj.turretTurnRate * Time.deltaTime)
            {
                step = rotateDir * thisObj.turretTurnRate * Time.deltaTime;

                thisObj.tsf_Turret.Rotate(0, step, 0);
                return;
            }
            else
            {
                step = angle;

                thisObj.tsf_Turret.Rotate(0, step, 0);

                if (functionTimeElapsed > 0) return;
            }
        }

        if (thisObj.curSelectedComp != this) return;

        Debug.Log("Attack");

        FunctionTriggered(thisObj.curSelectedFunction);

        BaseObj attackTarget = targetTile.GetEntitynThisTile();

        switch ((WeaponProjectileType)thisObj.curSelectedFunction.functionIntVal[3])
        {
            default:
                {
                    break;
                }
            case WeaponProjectileType.Laser:
                {
                    if (attackTarget == null) return;

                    var laserInstance = (GameObject)Resources.Load("Prefabs/Projectile/LaserBeam");
                    if (laserInstance != null)
                    {
                        var laserBeam = ObjectPool.Instance.CreateObject("LaserBeam", laserInstance, this.gameObject.transform.position, this.gameObject.transform.rotation).GetComponent<Proj_LaerBeam>();

                        laserBeam.TriggerThis(tsf_InstalledSlot.position, attackTarget.gameObject.transform.position, new Color(1, 0, 0, 0.75f));
                    }
                    attackTarget.TakeDamage(thisObj.curSelectedFunction.functionValue, WeaponAttackType.Pierce);
                    break;
                }
            case WeaponProjectileType.CurveProjectile:
                {
                    StartCoroutine(CreateProjectile(targetTile, true));
                    break;
                }
            case WeaponProjectileType.StraightProjectile:
                {
                    StartCoroutine(CreateProjectile(targetTile, false));
                    break;
                }
            case WeaponProjectileType.Melee:
                {
                    if (attackTarget == null) return;

                    attackTarget.TakeDamage(thisObj.curSelectedFunction.functionValue, WeaponAttackType.Pierce);
                    break;
                }
        }
        attackTarget = null;

    }
    IEnumerator CreateProjectile(BaseTile target, bool curve)
    {
        //var tile = target.GetTileWhereUnitIs();
        var tiles = Tools.GetTileWithinRange(target, thisObj.curSelectedFunction.functionIntVal[2], Tools.IgnoreType.All);

        List<BaseTile> tilesWeight = new List<BaseTile>();
        foreach (var item in tiles)
        {
            var weight = thisObj.curSelectedFunction.functionIntVal[2] - Tools.GetDistance(target.Pos, item.Pos) + 1;
            for (int i = 0; i < weight; i++)
            {
                tilesWeight.Add(item);
            }
        }
        for (int i = 0; i < thisObj.curSelectedFunction.functionIntVal[4]; i++)
        {
            var targetTile = Random.Range(0, tilesWeight.Count);
            Vector3 destination = tilesWeight[targetTile].transform.position;

            var projectile = (GameObject)Resources.Load("Prefabs/Projectile/Ballistic");
            if (projectile != null)
            {
                var proj = ObjectPool.Instance.CreateObject("Ballistic", projectile, this.gameObject.transform.position, this.gameObject.transform.rotation).GetComponent<Proj_Ballistic>();

                proj.transform.SetParent(MapController.Instance.tsf_ProjectileContainer, true);
                proj.InitThis(tsf_InstalledSlot, destination, thisObj, thisObj.curSelectedFunction.functionFloatVal[1], !curve, thisObj.curSelectedFunction.functionValue, 0);
            }
            yield return new WaitForSeconds(thisObj.curSelectedFunction.functionFloatVal[0]);
        }
    }
    #endregion
    #region Storage
    public ItemData ReceiveItem(ItemData receivedItem)
    {
        //SO_ItemData itemInfo = DataController.Instance.GetItemInfo(receivedItem.itemID);
        var itemData = DataController.Instance.GetItemData(receivedItem.itemID);

        int index = 0;
        do
        {
            if (index >= thisObj.inventory.Count && index < thisObj.maxStorageSlot)
            {
                var rec = new ItemData();
                rec.itemID = receivedItem.itemID;
                rec.stackCount = 0;
                thisObj.inventory.Add(rec);
            }

            if (index < thisObj.inventory.Count)
            {
                if (thisObj.inventory[index].itemID == receivedItem.itemID)
                {
                    if (thisObj.inventory[index].stackCount + receivedItem.stackCount <= itemData.maxStackCount)
                    {
                        SetInvCount(index, thisObj.inventory[index].stackCount + receivedItem.stackCount);
                        receivedItem.stackCount = 0;
                    }
                    else
                    {
                        var stackDiv = itemData.maxStackCount - thisObj.inventory[index].stackCount;
                        SetInvCount(index, itemData.maxStackCount);
                        receivedItem.stackCount -= stackDiv;
                    }
                }
            }

            index++;
        } while (receivedItem.stackCount > 0 && index <= thisObj.maxStorageSlot);

        return receivedItem;
    }
    public ItemData TransferItem(CompStorage targetStorage, ItemData transferedItem)
    {
        for (int i = thisObj.inventory.Count - 1; i >= 0; i--)
        {
            if (thisObj.inventory[i].itemID == transferedItem.itemID)
            {
                if (thisObj.inventory[i].stackCount <= transferedItem.stackCount)
                {
                    ItemData dataTemp = new ItemData();
                    dataTemp.itemID = thisObj.inventory[i].itemID;
                    dataTemp.stackCount = thisObj.inventory[i].stackCount;

                    var itemData = targetStorage.ReceiveItem(dataTemp);
                    transferedItem.stackCount -= thisObj.inventory[i].stackCount;
                    if (itemData.stackCount <= 0)
                    {
                        SetInvCount(i, 0);
                    }
                    else
                    {
                        SetInvCount(i, itemData.stackCount);
                    }
                }
                else
                {
                    var itemCount = transferedItem.stackCount;
                    var itemData = targetStorage.ReceiveItem(transferedItem);
                    var itemTransfered = itemCount - itemData.stackCount;

                    transferedItem.stackCount -= itemTransfered;
                    SetInvCount(i, thisObj.inventory[i].stackCount - itemTransfered);
                }
            }
            if (thisObj.inventory[i].stackCount <= 0)
            {
                thisObj.inventory.RemoveAt(i);
            }
            if (transferedItem.stackCount <= 0) break;
        }
        return transferedItem;
    }
    public int GetItemCount(string itemID)
    {
        int result = 0;
        foreach (var item in thisObj.inventory)
        {
            if (item.itemID == itemID)
            {
                result += item.stackCount;
            }
        }
        return result;
    }
    public void RemoveItem(ItemData itemInfo)
    {
        for (int i = thisObj.inventory.Count - 1; i >= 0; i--)
        {
            if (thisObj.inventory[i].itemID == itemInfo.itemID)
            {
                if (thisObj.inventory[i].stackCount <= itemInfo.stackCount)
                {
                    ItemData dataTemp = new ItemData();
                    dataTemp.itemID = thisObj.inventory[i].itemID;
                    dataTemp.stackCount = thisObj.inventory[i].stackCount;

                    itemInfo.stackCount -= thisObj.inventory[i].stackCount;
                    SetInvCount(i, 0);
                }
                else
                {
                    SetInvCount(i, thisObj.inventory[i].stackCount - itemInfo.stackCount);
                    itemInfo.stackCount = 0;
                }
            }
            if (thisObj.inventory[i].stackCount <= 0)
            {
                thisObj.inventory.RemoveAt(i);
            }
            if (itemInfo.stackCount <= 0) break;
        }
    }
    void SetInvCount(int index, int count)
    {
        ItemData temp = new ItemData();
        temp.itemID = thisObj.inventory[index].itemID;
        temp.stackCount = count;
        thisObj.inventory[index] = temp;
    }
    #endregion
    #region Construct
    IEnumerator constructItem()
    {
        var obj = DataController.Instance.GetEntityData(thisCompData.functions[curSelectedIndex].functionStringVal[0]);
        var objGenerated = GameObject.Instantiate(obj, MapController.Instance.entityContainer);
        objGenerated.InitThis();
        yield return null;
        bool check = false;
        foreach (var adjTile in thisObj.GetTileWhereUnitIs().adjacentTiles.Values)
        {
            if (obj.CheckIsTileSuitableForUnit(adjTile))
            {
                MapController.Instance.RegisterObject(objGenerated);
                objGenerated.Faction = thisObj.Faction;
                objGenerated.Pos = adjTile.Pos;
                objGenerated.gameObject.transform.position = adjTile.gameObject.transform.position;
                objGenerated.gameObject.SetActive(true);
                objGenerated.curTile = adjTile;
                adjTile.curObj = objGenerated;
                check = true;
                break;
            }
        }
        if (!check)
        {
            Destroy(objGenerated.gameObject);
        }
    }
    #endregion
    #endregion
}
