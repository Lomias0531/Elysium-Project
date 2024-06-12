using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class BaseResource : BaseObj
{
    public List<GameObject> objPos = new List<GameObject>();
    public enum ResourceType
    {
        Tree,
        Rock,
        Iron,
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitResource(Vector3Int pos, ResourceType type)
    {
        Pos = pos;
        Guid id = Guid.NewGuid();
        this.ID = id.ToString();

        for(int i = 0;i<3;i++)
        {
            GameObject obj;
            switch (type)
            {
                case ResourceType.Tree:
                    {
                        var index = Random.Range(0, MapController.Instance.treesTemplate.Count);
                        obj = GameObject.Instantiate(MapController.Instance.treesTemplate[index], objPos[i].transform);
                        obj.transform.localPosition = Vector3.zero;
                        break;
                    }
                case ResourceType.Rock:
                    {
                        var index = Random.Range(0, MapController.Instance.rocksTemplate.Count);
                        obj = GameObject.Instantiate(MapController.Instance.rocksTemplate[index], objPos[i].transform);
                        obj.transform.localPosition = Vector3.zero;
                        break;
                    }
                case ResourceType.Iron:
                    {
                        var index = Random.Range(0, MapController.Instance.metalTemplate.Count);
                        obj = GameObject.Instantiate(MapController.Instance.metalTemplate[index], objPos[i].transform);
                        obj.transform.localPosition = Vector3.zero;
                        break;
                    }
            }
        }
    }

    public override void OnBeingDestroyed()
    {
        
    }
    public override void OnInteracted()
    {
        foreach (var comp in components)
        {
            comp.OnApply();
        }
    }
}
