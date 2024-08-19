using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompConstructor : BaseComponent
{
    float constructTimeRequired;
    float constructTimeElapsed;
    public bool isConstructing;
    int curSelectedIndex = 0;
    public Image img_Progress;
    public Canvas canvas;

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
        if (storage != null)
        {
            for (int i = 1; i < functions[index].functionStringVal.Length; i++)
            {
                if (storage.GetItemCount(functions[index].functionStringVal[i]) < functions[index].functionFloatVal[i])
                {
                    checkResources = false;
                }
            }
        }
        else
        {
            checkResources = false;
        }

        int availableTileCount = 0;
        var obj = DataController.Instance.GetEntityViaID(functions[curSelectedIndex].functionStringVal[0]);
        foreach (var adjTile in thisObj.GetTileWhereUnitIs().adjacentTiles)
        {
            if(obj.CheckIsTileSuitableForUnit(adjTile.Value))
            {
                availableTileCount += 1;
            }
        }
        if (availableTileCount <= 0)
        {
            checkResources = false;
        }

        if (checkResources)
        {
            curSelectedIndex = index;
            isConstructing = true;
            for (int i = 1; i < functions[index].functionStringVal.Length; i++)
            {
                ItemData item = new ItemData();
                item.itemID = functions[index].functionStringVal[i];
                item.stackCount = (int)functions[index].functionFloatVal[i];

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
        canvas.worldCamera = Camera.main;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if(isConstructing)
        {
            this.EP -= functions[curSelectedIndex].functionConsume * Time.deltaTime;
            if (this.EP < 0)
            {
                this.EP = 0;
            }else
            {
                if (constructTimeElapsed < constructTimeRequired)
                {
                    constructTimeElapsed += Time.deltaTime;
                }
                else
                {
                    StartCoroutine(constructItem());
                }
            }
        }
        if(img_Progress!=null)
        {
            img_Progress.fillAmount = constructProgress;
        }
    }
    IEnumerator constructItem()
    {
        isConstructing = false;
        constructTimeElapsed = 0;
        var obj = DataController.Instance.GetEntityViaID(functions[curSelectedIndex].functionStringVal[0]);
        var objGenerated = GameObject.Instantiate(obj, MapController.Instance.entityContainer);
        objGenerated.InitThis();
        yield return null;
        bool check = false;
        foreach (var adjTile in thisObj.GetTileWhereUnitIs().adjacentTiles.Values)
        {
            if (obj.CheckIsTileSuitableForUnit(adjTile))
            {
                MapController.Instance.RegisterObject(objGenerated);
                objGenerated.Faction = thisObj.Faction;
                objGenerated.Pos = adjTile.Pos;
                objGenerated.gameObject.transform.position = adjTile.gameObject.transform.position;
                objGenerated.gameObject.SetActive(true);
                objGenerated.curTile = adjTile;
                adjTile.curObj = objGenerated;
                check = true;
                break;
            }
        }
        if(!check)
        {
            Destroy(objGenerated.gameObject);
        }
    }

    public override void OnTriggerFunction(params object[] obj)
    {
        
    }
}
