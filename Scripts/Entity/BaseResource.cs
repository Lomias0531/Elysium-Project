using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseResource : BaseObj
{
    public List<GameObject> objPos = new List<GameObject>();
    public enum ResourceType
    {
        Tree,
        Rock,
        Iron,
    }
    public override void OnInteracted()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitResource(ResourceType type)
    {
        switch(type)
        {
            case ResourceType.Tree:
                {
                    break;
                }
            case ResourceType.Rock:
                {
                    break;
                }
            case ResourceType.Iron:
                {
                    break;
                }
        }
    }
}
