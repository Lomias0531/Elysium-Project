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
        //switch(resourceType)
        //{
        //    default:
        //        {
        //            break;
        //        }
        //    case BaseResource.ResourceType.Tree:
        //        {
        //            PlayerDataManager.Instance.OrganicAmount += ResourceAcquireAmount;
        //            break;
        //        }
        //    case BaseResource.ResourceType.Rock:
        //        {
        //            PlayerDataManager.Instance.ConstructMaterialAmount += ResourceAcquireAmount;
        //            break;
        //        }
        //    case BaseResource.ResourceType.Iron:
        //        {
        //            PlayerDataManager.Instance.MetalAmount += ResourceAcquireAmount;
        //            break;
        //        }
        //}
    }

    public override void OnDestroyThis()
    {
        //switch (resourceType)
        //{
        //    default:
        //        {
        //            break;
        //        }
        //    case BaseResource.ResourceType.Tree:
        //        {
        //            PlayerDataManager.Instance.OrganicAmount += ResourceAcquireAmount * 5f;
        //            break;
        //        }
        //    case BaseResource.ResourceType.Rock:
        //        {
        //            PlayerDataManager.Instance.ConstructMaterialAmount += ResourceAcquireAmount * 5f;
        //            break;
        //        }
        //    case BaseResource.ResourceType.Iron:
        //        {
        //            PlayerDataManager.Instance.MetalAmount += ResourceAcquireAmount * 5f;
        //            break;
        //        }
        //}
        
        if(MapController.Instance.entityDic.ContainsKey(thisUnit.ID))
        {
            MapController.Instance.entityDic.Remove(thisUnit.ID);
        }
        Destroy(this.gameObject);
    }

    public override void OnInteract(BaseObj invoker)
    {
        //var storage = invoker.gameObject.GetComponent<CompStorage>();
        var storage = invoker.GetDesiredComponent<CompStorage>();
        if(storage != null)
        {
            ItemData data = new ItemData();
            data.itemID = resourceCollectableOnce.itemID;
            data.stackCount = resourceCollectableOnce.stackCount;
            storage.ReceiveItem(data);
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
                 function = ComponentFunction.Interact,
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
