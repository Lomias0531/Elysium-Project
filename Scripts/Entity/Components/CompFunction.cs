using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BaseObj;
using static CompWeapon;

public class CompFunction : BaseComponent
{
    Coroutine attactCoroutine;
    BaseObj attackTarget;

    public int maxStorageSlot;
    public List<ItemData> inventory = new List<ItemData>();
    public override void OnApply(int index)
    {
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
        }
    }

    public override void OnDestroyThis()
    {
        
    }

    public void OnTriggerFunction(ComponentFunctionType type, params object[] obj)
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

                    if ((BaseObj)obj[0] != null)
                    {
                        attackTarget = (BaseObj)obj[0];

                        CommenceAttack(attackTarget);
                    }
                    break;
                }
            case ComponentFunctionType.Resource:
                {
                    if (obj[0] is BaseObj)
                    {
                        var storage = ((BaseObj)obj[0]).GetDesiredComponent<CompStorage>();
                        if (storage != null)
                        {
                            for(int i = 0;i<thisObj.curSelectedFunction.functionStringVal.Length;i++)
                            {
                                ItemData data = new ItemData();
                                data.itemID = thisObj.curSelectedFunction.functionStringVal[i];
                                data.stackCount = thisObj.curSelectedFunction.functionIntVal[i];
                                storage.ReceiveItem(data);
                            }
                            //ItemData data = new ItemData();
                            //data.itemID = resourceCollectableOnce.itemID;
                            //data.stackCount = resourceCollectableOnce.stackCount;
                            //storage.ReceiveItem(data);
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

                        for (int i = inventory.Count - 1; i >= 0; i--)
                        {
                            if (inventory[i].itemID == transferedItem.itemID)
                            {
                                if (inventory[i].stackCount <= transferedItem.stackCount)
                                {
                                    ItemData dataTemp = new ItemData();
                                    dataTemp.itemID = inventory[i].itemID;
                                    dataTemp.stackCount = inventory[i].stackCount;

                                    var itemData = targetStorage.ReceiveItem(dataTemp);
                                    transferedItem.stackCount -= inventory[i].stackCount;
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
                                    SetInvCount(i, inventory[i].stackCount - itemTransfered);
                                }
                            }
                            if (inventory[i].stackCount <= 0)
                            {
                                inventory.RemoveAt(i);
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
    public void CommenceAttack(BaseObj target)
    {
        if (target == null)
        {
            var cpu = thisObj.GetDesiredComponent<CompAutoController>();
            if (cpu != null) cpu.ReceiveActionException(CompAutoController.UnitActException.IllegalAttack);
            return;
        }
        attackTarget = target;
        if (EP < thisObj.curSelectedFunction.functionConsume) return;

        if (thisObj.tsf_Turret != null)
        {
            var dir = thisObj.gameObject.transform.position - attackTarget.gameObject.transform.position;
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

        switch ((WeaponProjectileType)thisObj.curSelectedFunction.functionIntVal[3])
        {
            default:
                {
                    break;
                }
            case WeaponProjectileType.Laser:
                {
                    var laserInstance = (GameObject)Resources.Load("Prefabs/Projectile/LaserBeam");
                    if (laserInstance != null)
                    {
                        var laserBeam = ObjectPool.Instance.CreateObject("LaserBeam", laserInstance, this.gameObject.transform.position, this.gameObject.transform.rotation).GetComponent<Proj_LaerBeam>();

                        var index = Random.Range(0, thisObj.tsf_FirePos.Length);

                        laserBeam.TriggerThis(thisObj.tsf_FirePos[index].position, attackTarget.gameObject.transform.position, new Color(1, 0, 0, 0.75f));
                    }
                    attackTarget.TakeDamage(thisObj.curSelectedFunction.functionValue, WeaponAttackType.Pierce);
                    break;
                }
            case WeaponProjectileType.CurveProjectile:
                {
                    StartCoroutine(CreateProjectile(attackTarget, true));
                    break;
                }
            case WeaponProjectileType.StraightProjectile:
                {
                    StartCoroutine(CreateProjectile(attackTarget, false));
                    break;
                }
            case WeaponProjectileType.Melee:
                {
                    attackTarget.TakeDamage(thisObj.curSelectedFunction.functionValue, WeaponAttackType.Pierce);
                    break;
                }
        }
        attackTarget = null;

    }
    IEnumerator CreateProjectile(BaseObj target, bool curve)
    {
        var tile = target.GetTileWhereUnitIs();
        var tiles = Tools.GetTileWithinRange(tile, thisObj.curSelectedFunction.functionIntVal[2], Tools.IgnoreType.All);

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
                var index = Random.Range(0, thisObj.tsf_FirePos.Length);
                proj.InitThis(thisObj.tsf_FirePos[index], destination, thisObj, thisObj.curSelectedFunction.functionFloatVal[1], !curve, thisObj.curSelectedFunction.functionValue, 0);
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
            if (index >= inventory.Count && index < maxStorageSlot)
            {
                var rec = new ItemData();
                rec.itemID = receivedItem.itemID;
                rec.stackCount = 0;
                inventory.Add(rec);
            }

            if (index < inventory.Count)
            {
                if (inventory[index].itemID == receivedItem.itemID)
                {
                    if (inventory[index].stackCount + receivedItem.stackCount <= itemData.maxStackCount)
                    {
                        SetInvCount(index, inventory[index].stackCount + receivedItem.stackCount);
                        receivedItem.stackCount = 0;
                    }
                    else
                    {
                        var stackDiv = itemData.maxStackCount - inventory[index].stackCount;
                        SetInvCount(index, itemData.maxStackCount);
                        receivedItem.stackCount -= stackDiv;
                    }
                }
            }

            index++;
        } while (receivedItem.stackCount > 0 && index <= maxStorageSlot);

        return receivedItem;
    }
    public ItemData TransferItem(CompStorage targetStorage, ItemData transferedItem)
    {
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].itemID == transferedItem.itemID)
            {
                if (inventory[i].stackCount <= transferedItem.stackCount)
                {
                    ItemData dataTemp = new ItemData();
                    dataTemp.itemID = inventory[i].itemID;
                    dataTemp.stackCount = inventory[i].stackCount;

                    var itemData = targetStorage.ReceiveItem(dataTemp);
                    transferedItem.stackCount -= inventory[i].stackCount;
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
                    SetInvCount(i, inventory[i].stackCount - itemTransfered);
                }
            }
            if (inventory[i].stackCount <= 0)
            {
                inventory.RemoveAt(i);
            }
            if (transferedItem.stackCount <= 0) break;
        }
        return transferedItem;
    }
    public int GetItemCount(string itemID)
    {
        int result = 0;
        foreach (var item in inventory)
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
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].itemID == itemInfo.itemID)
            {
                if (inventory[i].stackCount <= itemInfo.stackCount)
                {
                    ItemData dataTemp = new ItemData();
                    dataTemp.itemID = inventory[i].itemID;
                    dataTemp.stackCount = inventory[i].stackCount;

                    itemInfo.stackCount -= inventory[i].stackCount;
                    SetInvCount(i, 0);
                }
                else
                {
                    SetInvCount(i, inventory[i].stackCount - itemInfo.stackCount);
                    itemInfo.stackCount = 0;
                }
            }
            if (inventory[i].stackCount <= 0)
            {
                inventory.RemoveAt(i);
            }
            if (itemInfo.stackCount <= 0) break;
        }
    }
    void SetInvCount(int index, int count)
    {
        ItemData temp = new ItemData();
        temp.itemID = inventory[index].itemID;
        temp.stackCount = count;
        inventory[index] = temp;
    }
    #endregion
    #endregion
}
