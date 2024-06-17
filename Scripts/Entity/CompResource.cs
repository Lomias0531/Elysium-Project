using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompResource : BaseComponent
{
    public BaseUnit thisUnit;
    public BaseResource.ResourceType resourceType;
    public float ResourceAcquireAmount;
    public override void OnApply()
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
        thisUnit = GetComponent<BaseUnit>();

        this.functions = new ComponentFunction[1]
        {
             ComponentFunction.Interact,
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
