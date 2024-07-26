using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor.UIElements;
using UnityEngine;

public class CompAutoController : BaseComponent
{
    BaseObj curAttackingTarget;
    BaseTile curMovingDestination;
    UnitActionStatus curStatus;
    bool inited = false;

    public int ScanRange;
    float actionTimeElapsed = 0;
    float actionTimeInterval = 0.5f;

    float statusTimeElapsed = 0;
    public enum UnitActException
    {
        None,
        IllegalMove,
        IllegalAttack,
        IllegalInteract,
    }
    public enum UnitActionStatus
    {
        Idle,
        Moving,
        Attacking,
        Searching,
    }
    float defaultMobileRange
    {
        get
        {
            var mobile = thisObj.GetDesiredComponent<CompMobile>();
            if(mobile != null)
            {
                return mobile.functions[0].functionValue;
            }else
            {
                return 0;
            }
        }
    }
    float maxAttackRange
    {
        get
        {
            var AttackRange = 0;
            var weapons = thisObj.GetDesiredComponents<CompWeapon>();
            foreach (var weapon in weapons)
            {
                foreach (var range in weapon.functions)
                {
                    if (range.functionIntVal[1] > AttackRange)
                    {
                        AttackRange = range.functionIntVal[1];
                    }
                }
            }
            return AttackRange;
        }
    }
    public override void OnApply(int index)
    {
        
    }

    public override void OnDestroyThis()
    {
        
    }

    public override void OnTriggerFunction(params object[] obj)
    {
        
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        inited = true;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (!inited) return;
        if (thisObj.isUniderConstruction) return;

        if(actionTimeElapsed > 0)
        {
            actionTimeElapsed -= Time.deltaTime;
            return;
        }

        switch(curStatus)
        {
            default:
                {
                    break;
                }
            case UnitActionStatus.Idle:
                {
                    Thread scanThread = new Thread(ScanForTarget);
                    scanThread.Start();
                    //ScanForTarget();
                    break;
                }
            case UnitActionStatus.Moving:
                {
                    var mobile = thisObj.GetDesiredComponent<CompMobile>();
                    if(mobile != null)
                    {
                        WayFinding();
                    }else
                    {
                        curStatus = UnitActionStatus.Attacking;
                    }
                    break;
                }
            case UnitActionStatus.Attacking:
                {
                    CommenceAttack();
                    break;
                }
            case UnitActionStatus.Searching:
                {
                    statusTimeElapsed += Time.deltaTime;
                    if (statusTimeElapsed > 2f)
                    {
                        Debug.Log("Reset status");
                        curStatus = UnitActionStatus.Idle;
                        statusTimeElapsed = 0;
                    }

                    break;
                }
        }
    }
    void ScanForTarget()
    {
        var closestDistance = Mathf.Infinity;
        string closestTarget = "";
        List<BaseObj> targets = new List<BaseObj>();
        var mobile = thisObj.GetDesiredComponent<CompMobile>();

        curStatus = UnitActionStatus.Searching;
        statusTimeElapsed = 0;

        var weapons = thisObj.GetDesiredComponents<CompWeapon>();
        if (weapons.Length <= 0) return;

        if (mobile != null)
        {
            foreach (var entity in MapController.Instance.entityDic)
            {
                if(entity.Value.Faction != thisObj.Faction)
                {
                    var distance = Tools.GetDistance(entity.Value.Pos, thisObj.Pos);
                    if(distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = entity.Value.EntityID;
                    }
                }
            }

            if(!string.IsNullOrEmpty(closestTarget))
            {
                curAttackingTarget = MapController.Instance.entityDic[closestTarget];

                if (weapons != null)
                {
                    if (closestDistance <= maxAttackRange)
                    {
                        curStatus = UnitActionStatus.Attacking;
                    }
                    else
                    {
                        curStatus = UnitActionStatus.Moving;
                    }
                }
                else
                {
                    curStatus = UnitActionStatus.Moving;
                }
            }
        }
        else
        {
            List<BaseTile> attackRange = new List<BaseTile>();

            foreach( var weapon in weapons)
            {
                foreach (var function in weapon.functions)
                {
                    var maxRange = Tools.GetTileWithinRange(thisObj.GetTileWhereUnitIs(), function.functionIntVal[1], Tools.IgnoreType.All);
                    var minRange = Tools.GetTileWithinRange(thisObj.GetTileWhereUnitIs(), function.functionIntVal[0], Tools.IgnoreType.All);
                    foreach (var tile in maxRange)
                    {
                        if(!attackRange.Contains(tile) && !minRange.Contains(tile))
                        {
                            attackRange.Add(tile);
                        }
                    }
                }
            }

            foreach (var tile in attackRange)
            {
                var entity = tile.GetEntitynThisTile();
                if (entity != null)
                {
                    if (entity.Faction == thisObj.Faction) continue;
                    var distance = Tools.GetDistance(thisObj.Pos,entity.Pos);
                    if(distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = entity.EntityID;
                    }
                }
            }

            if(!string.IsNullOrEmpty(closestTarget))
            {
                curAttackingTarget = MapController.Instance.entityDic[closestTarget];
                curStatus = UnitActionStatus.Attacking;
            }else
            {
                curAttackingTarget = null;
                curStatus = UnitActionStatus.Idle;
            }
        }
    }
    void WayFinding()
    {
        var mobile = thisObj.GetDesiredComponent<CompMobile>();

        if (mobile != null)
        {
            if (mobile.functionTimeElapsed > 0) return;
            if (mobile.EP < mobile.functions[0].functionConsume) return;
            var defaultMoveType = (BaseObj.MoveType)mobile.functions[0].functionIntVal[0];

            Queue<BaseTile> path = new Queue<BaseTile>();
            if ((curMovingDestination == null || thisObj.Pos == curMovingDestination.Pos) && curAttackingTarget.curTile != null)
            {
                var path1 = thisObj.UnitFindPath(curAttackingTarget.GetTileWhereUnitIs(), defaultMoveType, 100000);
                if(path1.Count <= 0)
                {
                    curStatus = UnitActionStatus.Idle;
                    actionTimeElapsed = actionTimeInterval;
                    return;
                }
                var maxRange = defaultMobileRange;
                if (path1.Count < maxRange) maxRange = path1.Count;
                for(int i  = 0;i< maxRange; i++)
                {
                    path.Enqueue(path1.Dequeue());
                }
                curMovingDestination = path.Last();
            }
            if(Tools.GetDistance(thisObj.Pos,curAttackingTarget.Pos) < maxAttackRange)
            {
                curStatus = UnitActionStatus.Attacking;
                return;
            }else
            {
                thisObj.curSelectedComp = mobile;
                thisObj.curSelectedFunction = mobile.functions[0];

                mobile.FunctionTriggered(mobile.functions[0]);
                StartCoroutine(mobile.MoveObject(path));
            }
        }
    }
    void CommenceAttack()
    {
        if(curAttackingTarget == null)
        {
            curStatus = UnitActionStatus.Idle;
            actionTimeElapsed = actionTimeInterval;
            return;
        }
        var targetDistance = Tools.GetDistance(curAttackingTarget.Pos, thisObj.Pos);
        var weapons = thisObj.GetDesiredComponents<CompWeapon>();

        if(targetDistance > maxAttackRange)
        {
            curStatus = UnitActionStatus.Idle;
            actionTimeElapsed = actionTimeInterval;
            return;
        }

        foreach (var weapon in weapons)
        {
            if (weapon.functionTimeElapsed > 0) continue;
            var randomList = new List<CompFunctionDetail>();
            var random = new System.Random();
            foreach (var item in weapon.functions.ToList())
            {
                randomList.Insert(random.Next(randomList.Count), item);
            }
            bool fired = false;
            for (int i = 0; i < randomList.Count; i++)
            {
                if (weapon.EP < randomList[i].functionConsume) continue;
                if (targetDistance > randomList[i].functionIntVal[1]) continue;
                if (targetDistance < randomList[i].functionIntVal[0]) continue;

                thisObj.curSelectedFunction = randomList[i];
                thisObj.curSelectedComp = weapon;

                weapon.FunctionTriggered(randomList[i]);
                weapon.CommenceAttack(curAttackingTarget);
                fired = true;
                break;
            }
            if (!fired)
            {
                curAttackingTarget = null;
            }
        }
    }
    public void ReceiveActionException(UnitActException exception)
    {
        switch(exception)
        {
            default:
                {
                    break;
                }
            case UnitActException.IllegalInteract:
                {
                    break;
                }
            case UnitActException.IllegalMove:
                {
                    curMovingDestination = null;
                    break;
                }
            case UnitActException.IllegalAttack:
                {
                    //curAttackingTarget = null;
                    break;
                }
        }
    }
}
