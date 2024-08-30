using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class HostileCommander : MonoBehaviour
{
    public string faction;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ControlUnits();
    }
    void ControlUnits()
    {
        int autoUnitsCount = 0;
        List<BaseObj> autoUnits = new List<BaseObj>();
        foreach (var entity in MapController.Instance.entityDic)
        {
            if(entity.Value.Faction == faction)
            {
                var construct = entity.Value.GetFunctionComponent(ComponentFunctionType.Construct);
                if(construct != null)
                {
                    if(construct.functionTimeElapsed <= 0)
                    {
                        var index = Random.Range(0, construct.thisCompData.functions.Length);
                        construct.FunctionTriggered(construct.thisCompData.functions[index]);
                        construct.OnApply(index);
                        entity.Value.curSelectedComp = construct;
                        entity.Value.curSelectedFunction = construct.thisCompData.functions[index];
                    }
                }
                var autoComp = entity.Value.GetDesiredComponent<CompAutoController>();
                if(autoComp != null)
                {
                    autoUnitsCount += 1;
                    autoUnits.Add(entity.Value);
                }
            }
        }

        if(autoUnitsCount <= 5)
        {
            foreach (var unit in autoUnits)
            {
                var autoComp = unit.GetDesiredComponent<CompAutoController>();
                if (autoComp != null)
                {
                    autoComp.SetActionMode(CompAutoController.UnitActionMode.Standby);
                }
            }
        }else
        {
            foreach (var unit in autoUnits)
            {
                var autoComp = unit.GetDesiredComponent<CompAutoController>();
                if (autoComp != null)
                {
                    autoComp.SetActionMode(CompAutoController.UnitActionMode.Agresssive);
                }
            }
        }
    }
}
