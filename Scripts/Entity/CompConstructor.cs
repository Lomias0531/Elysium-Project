using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompConstructor : BaseComponent
{
    float constructTimeRequired;
    float constructTimeElapsed;
    public bool isConstructing;
    public float constructProgress
    {
        get
        {
            if(constructTimeRequired == 0)
            {
                return 0;
            }
            return constructTimeElapsed / constructTimeRequired;
        }
    }
    public override void OnApply(int index)
    {
        if (isConstructing) return;
        CompStorage storage = thisObj.GetDesiredComponent<CompStorage>();
        bool checkResources = true;
        if(storage != null )
        {
            for(int i = 0;i< functions[index].functionStringVal.Length;i++)
            {
                if (storage.GetItemCount(functions[index].functionStringVal[i]) < functions[index].functionIntVal[i])
                {
                    checkResources = false;
                }
            }
        }else
        {
            checkResources = false;
        }
        if(checkResources)
        {
            isConstructing = true;
            for (int i = 0; i < functions[index].functionStringVal.Length; i++)
            {
                ItemData item = new ItemData();
                item.itemID = functions[index].functionStringVal[i];
                item.stackCount = functions[index].functionIntVal[i];

                storage.RemoveItem(item);
            }
            constructTimeElapsed = 0;
            constructTimeRequired = functions[index].functionFloatVal[0];
        }
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
        if(isConstructing)
        {
            if (constructTimeElapsed < constructTimeRequired)
            {
                constructTimeElapsed += Time.deltaTime;
            }
            else
            {
                isConstructing = false;

            }
        }
    }
}
