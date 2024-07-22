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

    public override void OnTriggerFunction(params object[] obj)
    {
        if(attactCoroutine != null)
            StopCoroutine(attactCoroutine);

        if((BaseObj)obj[0] != null)
        {
            attackTarget = (BaseObj)obj[0];
            //attactCoroutine = StartCoroutine(CommenceAttack());
            CommenceAttack(attackTarget);
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
        //CommenceAttack();
        CommenceAttack(attackTarget);
    }
    public void CommenceAttack(BaseObj target)
    {
        if (target == null) return;
        attackTarget = target;
        if (EP < thisObj.curSelectedFunction.functionConsume) return;

        var dir = thisObj.gameObject.transform.position - attackTarget.gameObject.transform.position;
        dir.y = 0;

        float angle = Vector3.SignedAngle(tsf_Turret.forward, dir, Vector3.up);
        float rotateDir = Mathf.Sign(angle);

        float step;

        if(angle >= rotateDir * turretTurnRate * Time.deltaTime)
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
                        attackTarget.TakeDamage(thisObj.curSelectedFunction.functionFloatVal[0], WeaponAttackType.Pierce);
                        break;
                    }
                case WeaponProjectileType.CurveProjectile:
                    {
                        break;
                    }
                case WeaponProjectileType.StraightProjectile:
                    {
                        break;
                    }
                case WeaponProjectileType.Melee:
                    {
                        attackTarget.TakeDamage(thisObj.curSelectedFunction.functionFloatVal[0], WeaponAttackType.Pierce);
                        break;
                    }
            }
            attackTarget = null;
        }
    }
}
