using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class CompPowerDispathcer : BaseComponent
{
    public int powerRadiationRange;
    public float maxPowerDispathable;
    public LineRenderer powerProject;
    public GameObject obj_TransferStart;
    public override void OnApply(int index)
    {
        FunctionTriggered(functions[index]);
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
        if (thisObj.GetDesiredComponent<CompConstructTemp>() != null) return;
        if (functionTimeElapsed <= 0)
        {
            foreach (var unit in MapController.Instance.entityDic.Values)
            {
                if(Tools.GetDistance(unit.Pos,thisObj.Pos) <= powerRadiationRange)
                {
                    if (unit == thisObj) continue;
                    if (unit.Faction == thisObj.Faction)
                    {
                        float powerTranfered = 0;
                        for (int i = 0; i < unit.components.Count; i++)
                        {
                            if (unit.components[i].EP / unit.components[i].MaxEP > 0.8f) continue;

                            var targetEPDis = unit.components[i].MaxEP - unit.components[i].EP;
                            if (targetEPDis <= 0) continue;
                            powerTranfered = targetEPDis;
                            if (targetEPDis >= maxPowerDispathable)
                            {
                                if (this.EP < maxPowerDispathable)
                                {
                                    unit.components[i].EP += this.EP;
                                    this.EP = 0;
                                }
                                else
                                {
                                    this.EP -= maxPowerDispathable;
                                    unit.components[i].EP += maxPowerDispathable;
                                }
                            }
                            else
                            {
                                if (this.EP < targetEPDis)
                                {
                                    unit.components[i].EP += this.EP;
                                    this.EP = 0;
                                }
                                else
                                {
                                    this.EP -= targetEPDis;
                                    unit.components[i].EP += targetEPDis;
                                }
                            }

                            StartCoroutine(DisplayPowerDispatcher(unit));
                            break;
                        }

                        if (powerTranfered > 0)
                        {
                            OnApply(0);
                            break;
                        }
                    }
                }
            }
        }
    }
    IEnumerator DisplayPowerDispatcher(BaseObj target)
    {
        var time = 0f;
        if(powerProject != null)
        {
            do
            {
                powerProject.enabled = true;
                Vector3[] pos = new Vector3[2];
                pos[0] = obj_TransferStart.transform.position;
                pos[1] = target.transform.position;
                powerProject.SetPositions(pos);
                var width = (0.25f - Mathf.Abs(time - 0.25f)) / 0.25f * 0.06f;
                powerProject.startWidth = width;
                powerProject.endWidth = width;

                yield return null;

                time += Time.deltaTime;
            } while (time < 0.5f);

            powerProject.enabled = false;
        }
    }
}
