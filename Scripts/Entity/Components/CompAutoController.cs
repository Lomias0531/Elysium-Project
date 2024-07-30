using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using System.Collections;
using static UnityEngine.AudioSettings;

[BurstCompile]
public class CompAutoController : BaseComponent
{
    BaseObj curAttackingTarget;
    BaseTile curMovingDestination;
    UnitActionStatus curStatus = UnitActionStatus.Searching;
    UnitActionMode curMode = UnitActionMode.Protective;
    bool inited = false;

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
    public enum UnitActionMode
    {
        Standby,
        Agresssive,
        Protective,
        Passive,
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
                    ScanForTarget();
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
        var mobile = thisObj.GetDesiredComponent<CompMobile>();

        curStatus = UnitActionStatus.Searching;
        statusTimeElapsed = 0;

        var weapons = thisObj.GetDesiredComponents<CompWeapon>();
        if (weapons.Length <= 0) return;

        if (curMode == UnitActionMode.Passive) return;

        List<TargetAnalysis> targets = new List<TargetAnalysis>();

        if (mobile != null && curMode == UnitActionMode.Agresssive)
        {
            foreach (var entity in MapController.Instance.entityDic)
            {
                if(entity.Value.Faction != thisObj.Faction)
                {
                    if (curMode != UnitActionMode.Standby && entity.Value.Faction == "Resource") continue;
                    var distance = Tools.GetDistance(entity.Value.Pos, thisObj.Pos);

                    TargetAnalysis target = new TargetAnalysis();
                    target.ID = entity.Value.EntityID;
                    target.distance = distance;
                    target.threat = entity.Value.Faction == "Resource" ? 0 : 20;
                    targets.Add(target);
                }
            }

            targets.OrderBy(x => (100 - x.distance) + x.threat);
            var index = targets.Count > 5 ? Random.Range(0, 5) : Random.Range(0, targets.Count);
            var closestTarget = targets[index].ID;
            var closestDistance = targets[index].distance;

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

                    TargetAnalysis target = new TargetAnalysis();
                    target.ID = entity.EntityID;
                    target.distance = distance;
                    target.threat = entity.Faction == "Resource" ? 0 : 20;
                    targets.Add(target);
                }
            }

            targets.OrderBy(x => (100 - x.distance) + x.threat);
            var index = targets.Count > 5 ? Random.Range(0, 5) : Random.Range(0, targets.Count);
            var closestTarget = targets[index].ID;
            var closestDistance = targets[index].distance;

            if (!string.IsNullOrEmpty(closestTarget))
            {
                curAttackingTarget = MapController.Instance.entityDic[closestTarget];
                curStatus = UnitActionStatus.Attacking;
            }else
            {
                curAttackingTarget = null;
                if(curMode == UnitActionMode.Standby)
                {
                    curStatus = UnitActionStatus.Moving;
                }
                else
                {
                    curStatus = UnitActionStatus.Idle;
                }
            }
        }
    }
    void WayFinding()
    {
        var mobile = thisObj.GetDesiredComponent<CompMobile>();

        if (curAttackingTarget == null && curMode != UnitActionMode.Standby)
        {
            curStatus = UnitActionStatus.Idle;
            return;
        }

        if (mobile != null)
        {
            if (mobile.functionTimeElapsed > 0) return;
            if (mobile.EP < mobile.functions[0].functionConsume) return;
            var defaultMoveType = (BaseObj.MoveType)mobile.functions[0].functionIntVal[0];

            Queue<BaseTile> path = new Queue<BaseTile>();
            if (curMovingDestination == null || thisObj.Pos == curMovingDestination.Pos)
            {
                if(curMode != UnitActionMode.Standby)
                {
                    if(curAttackingTarget == null)
                    {
                        curStatus = UnitActionStatus.Idle;
                        actionTimeElapsed = actionTimeInterval;
                        return;
                    }
                    var path1 = thisObj.UnitFindPath(curAttackingTarget.GetTileWhereUnitIs(), defaultMoveType, 100000);
                    if (path1.Count <= 0)
                    {
                        curStatus = UnitActionStatus.Idle;
                        actionTimeElapsed = actionTimeInterval;
                        return;
                    }
                    var maxRange = defaultMobileRange;
                    if (path1.Count < maxRange) maxRange = path1.Count;
                    for (int i = 0; i < maxRange; i++)
                    {
                        path.Enqueue(path1.Dequeue());
                    }
                    curMovingDestination = path.Last();
                }else
                {
                    var mobileTile = Tools.GetTileWithinRange(thisObj.curTile, 2, Tools.IgnoreType.None);
                    int targetIndex;
                    bool targetOK = false;
                    do
                    {
                        targetIndex = Random.Range(0, mobileTile.Count);
                        if (thisObj.CheckIsTileSuitableForUnit(mobileTile[targetIndex]))
                        {
                            targetOK = true;
                        }
                    } while (!targetOK);

                    curMovingDestination = mobileTile[targetIndex];
                }
            }

            if (curAttackingTarget == null && curMode == UnitActionMode.Standby)
            {
                thisObj.curSelectedComp = mobile;
                thisObj.curSelectedFunction = mobile.functions[0];

                mobile.FunctionTriggered(mobile.functions[0]);
                StartCoroutine(mobile.MoveObject(path));

                return;
            }
            if(Tools.GetDistance(thisObj.Pos,curAttackingTarget.Pos) < maxAttackRange)
            {
                curStatus = UnitActionStatus.Attacking;
                return;
            }else
            {

                if (path.Count <= 0)
                {
                    curMovingDestination = null;
                    return;
                }
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
                    MoveToAdjPos();
                    break;
                }
            case UnitActException.IllegalAttack:
                {
                    //curAttackingTarget = null;
                    break;
                }
        }
    }
    void MoveToAdjPos()
    {
        var mobileTile = Tools.GetTileWithinRange(thisObj.curTile, 2, Tools.IgnoreType.None);
        int targetIndex;
        bool targetOK = false;
        do
        {
            targetIndex = Random.Range(0, mobileTile.Count);
            if (thisObj.CheckIsTileSuitableForUnit(mobileTile[targetIndex]))
            {
                targetOK = true;
            }
        } while (!targetOK);

        curMovingDestination = mobileTile[targetIndex];
        var mobile = thisObj.GetDesiredComponent<CompMobile>();
        var path = thisObj.UnitFindPath(curMovingDestination, (BaseObj.MoveType)mobile.functions[0].functionIntVal[0], 4);

        thisObj.curSelectedComp = mobile;
        thisObj.curSelectedFunction = mobile.functions[0];

        mobile.FunctionTriggered(mobile.functions[0]);
        StartCoroutine(mobile.MoveObject(path));
    }
    public void SetActionMode(UnitActionMode mode)
    {
        curMode = mode;
    }
    struct TargetAnalysis
    {
        public string ID;
        public float distance;
        public float threat;
    }
}
