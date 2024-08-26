using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompWeapon : BaseComponent
{
    public Transform tsf_Turret;
    public float turretTurnRate;
    public Transform[] tsf_FirePos;

    BaseObj attackTarget;

    Coroutine attactCoroutine;
    public enum WeaponProjectileType
    {
        None,
        Laser,
        CurveProjectile,
        StraightProjectile,
        Melee,
    }
    public enum WeaponAttackType
    {
        None,
        Pierce,
        Blast,
    }
    public override void OnApply(int index)
    {
        PlayerController.Instance.GetAttackRange();
    }

    public override void OnDestroyThis()
    {
        
    }

    //public override void OnTriggerFunction(params object[] obj)
    //{
    //    if(attactCoroutine != null)
    //        StopCoroutine(attactCoroutine);

    //    if((BaseObj)obj[0] != null)
    //    {
    //        attackTarget = (BaseObj)obj[0];
    //        //attactCoroutine = StartCoroutine(CommenceAttack());
    //        CommenceAttack(attackTarget);
    //    }
    //}

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        //CommenceAttack();
        if(attackTarget != null)
            CommenceAttack(attackTarget);
    }
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

        if(tsf_Turret != null)
        {
            var dir = thisObj.gameObject.transform.position - attackTarget.gameObject.transform.position;
            dir.y = 0;

            float angle = Vector3.SignedAngle(tsf_Turret.forward, dir, Vector3.up);
            float rotateDir = Mathf.Sign(angle);

            float step;

            if (Mathf.Abs(angle) >= turretTurnRate * Time.deltaTime)
            {
                step = rotateDir * turretTurnRate * Time.deltaTime;

                tsf_Turret.Rotate(0, step, 0);
                return;
            }
            else
            {
                step = angle;

                tsf_Turret.Rotate(0, step, 0);

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

                        var index = Random.Range(0, tsf_FirePos.Length);

                        laserBeam.TriggerThis(tsf_FirePos[index].position, attackTarget.gameObject.transform.position, new Color(1, 0, 0, 0.75f));
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
            for(int i = 0;i< weight;i++)
            {
                tilesWeight.Add(item);
            }
        }
        for(int i = 0;i< thisObj.curSelectedFunction.functionIntVal[4];i++)
        {
            var targetTile = Random.Range(0, tilesWeight.Count);
            Vector3 destination = tilesWeight[targetTile].transform.position;

            var projectile = (GameObject)Resources.Load("Prefabs/Projectile/Ballistic");
            if (projectile != null)
            {
                var proj = ObjectPool.Instance.CreateObject("Ballistic", projectile, this.gameObject.transform.position, this.gameObject.transform.rotation).GetComponent<Proj_Ballistic>();

                proj.transform.SetParent(MapController.Instance.tsf_ProjectileContainer, true);
                var index = Random.Range(0, tsf_FirePos.Length);
                proj.InitThis(tsf_FirePos[index], destination, thisObj, thisObj.curSelectedFunction.functionFloatVal[1], !curve, thisObj.curSelectedFunction.functionValue, 0);
            }
            yield return new WaitForSeconds(thisObj.curSelectedFunction.functionFloatVal[0]);
        }
        //target.TakeDamage(thisObj.curSelectedFunction.functionFloatVal[0], WeaponAttackType.Blast);
    }
}
