using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CompAutoController : BaseComponent
{
    BaseObj curAttackingTarget;
    BaseTile curMovingDestination;
    public enum UnitActException
    {
        None,
        IllegalMove,
        IllegalAttack,
        IllegalInteract,
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

        if (curAttackingTarget == null)
        {
            ScanForTarget();
            return;
        }

        var mobile = thisObj.GetDesiredComponent<CompMobile>();
        if(mobile != null && curMovingDestination == null)
        {
            WayFinding();
            return;
        }

        var targetDistance = Tools.GetDistance(curAttackingTarget.Pos, thisObj.Pos);
        var weapons = thisObj.GetDesiredComponents<CompWeapon>();
        foreach(var weapon in weapons)
        {
            if (weapon.functionTimeElapsed > 0) continue;
            var randomList = new List<CompFunctionDetail>();
            var random = new System.Random();
            foreach (var item in weapon.functions.ToList())
            {
                randomList.Insert(random.Next(randomList.Count), item);
            }
            bool fired = false;
            for(int i = 0;i< randomList.Count;i++)
            {
                if (weapon.EP < randomList[i].functionConsume) continue;
                if (targetDistance > randomList[i].functionIntVal[1]) continue;
                if (targetDistance < randomList[i].functionIntVal[0]) continue;
                weapon.FunctionTriggered(randomList[i]);
                weapon.CommenceAttack(curAttackingTarget);
                fired = true;
                thisObj.curSelectedFunction = randomList[i];
                thisObj.curSelectedComp = weapon;
                break;
            }
            if(!fired)
            {
                curAttackingTarget = null;
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
        }
    }
    void WayFinding()
    {

    }
    void CommenceAttack()
    {

    }
    public void ReceiveActionException(UnitActException exception)
    {
        switch(exception)
        {
            default:
                {
                    break;
                }
        }
    }
}
