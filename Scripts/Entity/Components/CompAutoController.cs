using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CompAutoController : BaseComponent
{
    BaseObj curAttackingTarget;
    BaseTile curMovingDestination;
    UnitActionStatus curStatus;
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
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (thisObj.isUniderConstruction) return;

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
        }
    }
    void ScanForTarget()
    {
        var closestDistance = Mathf.Infinity;
        var closestTarget = "";
        foreach (var entity in MapController.Instance.entityDic)
        {
            if(entity.Value.Faction != thisObj.Faction)
            {
                if (entity.Value.HP <= 0) continue;
                var distance = Tools.GetDistance(entity.Value.Pos, thisObj.Pos);
                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = entity.Key;
                }
            }
        }


        if(MapController.Instance.entityDic.ContainsKey(closestTarget))
        {
            curAttackingTarget = MapController.Instance.entityDic[closestTarget];

            var weapons = thisObj.GetDesiredComponents<CompWeapon>();

            if (weapons != null)
            {
                if(Tools.GetDistance(thisObj.Pos,curAttackingTarget.Pos) <= maxAttackRange)
                {
                    curStatus = UnitActionStatus.Attacking;
                }else
                {
                    curStatus = UnitActionStatus.Moving;
                }
            }else
            {
                curStatus = UnitActionStatus.Moving;
            }
        }
    }
    void WayFinding()
    {
        var mobile = thisObj.GetDesiredComponent<CompMobile>();

        if(mobile != null)
        {
            if (mobile.functionTimeElapsed > 0) return;
            if (mobile.EP < mobile.functions[0].functionConsume) return;
            var defaultMoveType = (BaseObj.MoveType)mobile.functions[0].functionIntVal[0];

            List<BaseTile> path = new List<BaseTile>();
            if(curMovingDestination == null || thisObj.Pos == curMovingDestination.Pos)
            {
                var path1 = thisObj.UnitFindPath(curAttackingTarget.GetTileWhereUnitIs(), defaultMoveType);
                var maxRange = defaultMobileRange;
                if (path1.Count < maxRange) maxRange = path1.Count;
                for(int i  = 0;i< maxRange; i++)
                {
                    path.Add(path1.Dequeue());
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
                StartCoroutine(mobile.MoveObject(curMovingDestination));
            }
        }
    }
    void CommenceAttack()
    {
        if(curAttackingTarget == null)
        {
            curStatus = UnitActionStatus.Idle;
            return;
        }
        var targetDistance = Tools.GetDistance(curAttackingTarget.Pos, thisObj.Pos);
        var weapons = thisObj.GetDesiredComponents<CompWeapon>();

        if(targetDistance > maxAttackRange)
        {
            curStatus = UnitActionStatus.Idle;
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
                    curAttackingTarget = null;
                    break;
                }
        }
    }
}
