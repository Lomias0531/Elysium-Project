using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BaseObj;

public class CompMobile : BaseComponent
{
    public bool isMoving;
    public override void OnApply(int index)
    {
        var moveType = (BaseObj.MoveType)functions[index].functionIntVal[0];
        var moveStyle = (BaseObj.MoveStyle)functions[index].functionIntVal[1];
        var mobility = (int)functions[index].functionValue;

        PlayerController.Instance.GetMoveRange(Tools.GetMobileRange(thisObj, moveType, moveStyle, mobility));
    }

    public override void OnDestroyThis()
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
    }
    public IEnumerator MoveObject(BaseTile tile)
    {
        if (tile == null) yield break;
        isMoving = true;
        var moveQueue = thisObj.UnitFindPath(tile, (MoveType)thisObj.curSelectedFunction.functionIntVal[0], (int)thisObj.curSelectedFunction.functionValue);

        StartCoroutine(MoveObject(moveQueue));
    }

    public IEnumerator MoveObject(Queue<BaseTile> moveQueue)
    {
        if((moveQueue == null || moveQueue.Count == 0) && (MoveStyle)thisObj.curSelectedFunction.functionIntVal[1] == MoveStyle.Ordinary)
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
                        do
                        {
                            this.transform.position = Vector3.Lerp(originPos, target.transform.position, moveTimeElapsed / 0.2f);
                            moveTimeElapsed += Time.deltaTime;
                            yield return null;
                        } while (moveTimeElapsed < 0.2f);
                        this.transform.position = target.transform.position;
                        //this.transform.DOMove(target.transform.position, 0.2f, false);
                        thisObj.Pos = target.Pos;
                        //yield return new WaitForSeconds(0.2f);
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

                    break;
                }
        }

        PlayerController.Instance.EntityFinishedAction();

        if (thisObj.animator != null)
        {
            thisObj.animator.speed = 1f;
            thisObj.animator.CrossFadeInFixedTime("Idle", 0.1f);
        }
        yield return new WaitForSeconds(0.1f);

        isMoving = false;
    }

    public override void OnTriggerFunction(params object[] obj)
    {
        if(obj[0] is BaseTile)
        {
            StartCoroutine(MoveObject((BaseTile)obj[0]));
        }
    }
}
