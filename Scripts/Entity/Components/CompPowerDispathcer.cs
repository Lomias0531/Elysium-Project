using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class CompPowerDispathcer : BaseComponent
{
    public int powerRadiationRange;
    public float maxPowerDispathable;
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
        if(functionTimeElapsed <= 0)
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
}
