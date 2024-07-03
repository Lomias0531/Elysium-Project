using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompResource : BaseComponent
{
    public BaseObj thisUnit;
    public BaseResource.ResourceType resourceType;
    public float ResourceAcquireAmount;
    public override void OnApply(int index)
    {
        switch(resourceType)
        {
            default:
                {
                    break;
                }
            case BaseResource.ResourceType.Tree:
                {
                    PlayerDataManager.Instance.OrganicAmount += ResourceAcquireAmount;
                    break;
                }
            case BaseResource.ResourceType.Rock:
                {
                    PlayerDataManager.Instance.ConstructMaterialAmount += ResourceAcquireAmount;
                    break;
                }
            case BaseResource.ResourceType.Iron:
                {
                    PlayerDataManager.Instance.MetalAmount += ResourceAcquireAmount;
                    break;
                }
        }
    }

    public override void OnDestroyThis()
    {
        switch (resourceType)
        {
            default:
                {
                    break;
                }
            case BaseResource.ResourceType.Tree:
                {
                    PlayerDataManager.Instance.OrganicAmount += ResourceAcquireAmount * 5f;
                    break;
                }
            case BaseResource.ResourceType.Rock:
                {
                    PlayerDataManager.Instance.ConstructMaterialAmount += ResourceAcquireAmount * 5f;
                    break;
                }
            case BaseResource.ResourceType.Iron:
                {
                    PlayerDataManager.Instance.MetalAmount += ResourceAcquireAmount * 5f;
                    break;
                }
        }
        
        if(MapController.Instance.entityDic.ContainsKey(thisUnit.ID))
        {
            MapController.Instance.entityDic.Remove(thisUnit.ID);
        }
        Destroy(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
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
    void Update()
    {
        
    }
}
