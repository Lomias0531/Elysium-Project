using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using TMPro;

public abstract class BaseObj : MonoBehaviour
{
    public Vector3Int Pos;
    public string ID;
    Animator animator;
    [HideInInspector]
    public MoveType[] moveType
    {
        get
        {
            List<MoveType> list = new List<MoveType>();
            foreach (var comp in components)
            {
                if(comp.GetType() == typeof(CompMobile))
                {
                    foreach (var item in comp.functions)
                    {
                        list.Add((BaseUnit.MoveType)item.functionIntVal[0]);
                    }
                }
            }
            return list.ToArray();
        }
    }
    [HideInInspector]
    public MoveStyle[] moveStyle
    {
        get
        {
            List<MoveStyle> list = new List<MoveStyle>();
            foreach (var comp in components)
            {
                if (comp.GetType() == typeof(CompMobile))
                {
                    foreach (var item in comp.functions)
                    {
                        list.Add((BaseUnit.MoveStyle)item.functionIntVal[1]);
                    }
                }
            }
            return list.ToArray();
        }
    }
    public string objName;
    [HideInInspector]
    public List<BaseComponent> components = new List<BaseComponent>();
    public string Faction;
    public float HP
    {
        get
        {
            float value = 0f;
            foreach (var item in components)
            {
                value += item.HP;
            }
            return value;
        }
    }
    public float HPMax
    {
        get
        {
            float value = 0f;
            foreach (var item in components)
            {
                value += item.MaxHP;
            }
            return value;
        }
    }
    public float EP
    {
        get
        {
            float value = 0f;
            foreach (var item in components)
            {
                value += item.EP;
            }
            return value;
        }
    }
    public float EPMax
    {
        get
        {
            float value = 0f;
            foreach (var item in components)
            {
                value += item.MaxEP;
            }
            return value;
        }
    }

    public enum MoveType
    {
        None,
        Ground,
        Water,
        Air,
    }
    public enum MoveStyle
    {
        None,
        Ordinary,
        Jump,
        Teleport,
    }

    //public MoveType curSelectedMoveType;
    //public MoveStyle curSelectedMoveStyle;
    public BaseComponent curSelectedComp;
    public CompFunctionDetail curSelectedFunction;
    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }
    public virtual void InitThis()
    {
        Guid id = Guid.NewGuid();
        this.ID = id.ToString();

        var _components = this.gameObject.GetComponents<BaseComponent>();
        components = _components.ToList();
        foreach (var item in components)
        {
            item.thisObj = this;
        }
        animator = this.gameObject.GetComponent<Animator>();
    }
    public abstract void OnInteracted();
    public abstract void OnBeingDestroyed();

    public IEnumerator MoveObjectToTile(BaseTile tile)
    {
        var moveQueue = this.UnitFindPath(tile, (MoveType)curSelectedFunction.functionIntVal[0]);

        switch ((MoveStyle)curSelectedFunction.functionIntVal[1])
        {
            default:
                {
                    if(animator != null)
                    {
                        animator.CrossFadeInFixedTime("Walk", 0.1f);
                        yield return new WaitForSeconds(0.1f);
                    }

                    do
                    {
                        var target = moveQueue.Dequeue();
                        if (target == this.GetTileWhereUnitIs())
                            continue;

                        var facing = Vector3.Angle(new Vector3(0, 0, 1f), target.transform.position - this.transform.position);
                        if (target.transform.position.x < this.transform.position.x) facing *= -1;

                        this.transform.localEulerAngles = new Vector3(0, facing, 0);

                        float moveTimeElapsed = 0;
                        var originPos = this.transform.position;
                        do
                        {
                            this.transform.position = Vector3.Lerp(originPos, target.transform.position, moveTimeElapsed / 0.2f);
                            moveTimeElapsed += Time.deltaTime;
                            yield return null;
                        } while (moveTimeElapsed < 0.2f);
                        this.transform.position = target.transform.position;
                        //this.transform.DOMove(target.transform.position, 0.2f, false);
                        this.Pos = target.Pos;
                        //yield return new WaitForSeconds(0.2f);
                    } while (moveQueue.Count > 0);

                    break;
                }
            case MoveStyle.Teleport:
                {
                    if (animator != null)
                    {
                        animator.CrossFadeInFixedTime("Interact", 0.1f);
                        yield return new WaitForSeconds(0.2f);
                        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                    }

                    var facing = Vector3.Angle(new Vector3(0, 0, 1f), tile.transform.position - this.transform.position);
                    if (tile.transform.position.x < this.transform.position.x) facing *= -1;

                    this.transform.localEulerAngles = new Vector3(0, facing, 0);

                    //yield return new WaitForSeconds(0.2f);

                    this.transform.position = tile.transform.position;
                    this.Pos = tile.Pos;
                    break;
                }
            case MoveStyle.Jump:
                {
                    if (animator != null)
                    {
                        animator.CrossFadeInFixedTime("Jump", 0.1f);
                        yield return new WaitForSeconds(0.1f);
                    }

                    var facing = Vector3.Angle(new Vector3(0, 0, 1f), tile.transform.position - this.transform.position);
                    if (tile.transform.position.x < this.transform.position.x) facing *= -1;

                    this.transform.localEulerAngles = new Vector3(0, facing, 0);

                    float jumpTileElapsed = 0;
                    do
                    {
                        this.transform.position = Tools.GetBezierCurve(this.GetTileWhereUnitIs().transform.position, tile.transform.position, jumpTileElapsed / 1f);
                        jumpTileElapsed += Time.deltaTime;

                        yield return null;
                    } while (jumpTileElapsed <= 1f);
                    this.transform.position = tile.transform.position;
                    this.Pos = tile.Pos;

                    break;
                }
        }

        //PlayerController.Instance.CancelAllOperations();

        if (animator != null)
        {
            animator.CrossFadeInFixedTime("Idle", 0.1f);
        }
        yield return new WaitForSeconds(0.1f);
    }
}
