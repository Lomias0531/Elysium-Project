using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompWallConnector : BaseComponent
{
    public List<GameObject> DirWalls = new List<GameObject>();
    public override void OnApply(int index)
    {
        
    }

    public override void OnDestroyThis()
    {
        foreach (var item in DirWalls)
        {
            item.gameObject.SetActive(false);
        }
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
        foreach (var tile in thisObj.GetTileWhereUnitIs().adjacentTiles)
        {
            var unit = tile.Value.GetEntitynThisTile();
            if (unit != null)
            {
                var wallConnector = unit.GetDesiredComponent<CompWallConnector>();
                if (wallConnector != null)
                {
                    DirWalls[(int)tile.Key].gameObject.SetActive(true);
                }
                else
                {
                    DirWalls[(int)tile.Key].gameObject.SetActive(false);
                }
            }else
            {
                DirWalls[(int)tile.Key].gameObject.SetActive(false);
            }
        }
    }
}
