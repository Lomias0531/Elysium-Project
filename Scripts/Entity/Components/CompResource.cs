using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CompResource : CompInteractable
{
    public BaseObj thisUnit;
    public BaseResource.ResourceType resourceType;
    public float ResourceAcquireAmount;
    public ItemData resourceCollectableOnce;
    public override void OnApply(int index)
    {

    }

    public override void OnDestroyThis()
    {
     
        if(MapController.Instance.entityDic.ContainsKey(thisUnit.EntityID))
        {
            MapController.Instance.entityDic.Remove(thisUnit.EntityID);
        }
        Destroy(this.gameObject);
    }

    public override void OnInteract(BaseObj invoker)
    {

    }

    public override void OnTriggerFunction(params object[] obj)
    {
        if(obj[0] is BaseObj)
        {
            var storage = ((BaseObj)obj[0]).GetDesiredComponent<CompStorage>();
            if (storage != null)
            {
                ItemData data = new ItemData();
                data.itemID = resourceCollectableOnce.itemID;
                data.stackCount = resourceCollectableOnce.stackCount;
                storage.ReceiveItem(data);
            }
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        thisUnit = GetComponent<BaseObj>();

        this.functions = new CompFunctionDetail[1]
        {
             new CompFunctionDetail
             {
                 functionName = "采集",
                 //function = ComponentFunctionType.Interact,
                 functionValue = ResourceAcquireAmount,
                 functionConsume = 0,
                 functionDescription = "可以采集",
             }
        };
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
}
