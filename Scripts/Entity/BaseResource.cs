using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class BaseResource : BaseObj
{
    public enum ResourceType
    {
        Tree,
        Rock,
        Iron,
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
    }
    public void InitResource(Vector3Int pos, ResourceType type)
    {
        Pos = pos;
        Guid id = Guid.NewGuid();
        this.ID = id.ToString();

        GameObject obj;
        float rot = Random.Range(0f, 359f);
        switch (type)
        {
            case ResourceType.Tree:
                {
                    var index = Random.Range(0, MapController.Instance.treesTemplate.Count);
                    obj = GameObject.Instantiate(MapController.Instance.treesTemplate[index], this.transform);
                    this.objName = "��ľ";
                    break;
                }
            case ResourceType.Rock:
                {
                    var index = Random.Range(0, MapController.Instance.rocksTemplate.Count);
                    obj = GameObject.Instantiate(MapController.Instance.rocksTemplate[index], this.transform);
                    this.objName = "ʯͷ";
                    break;
                }
            case ResourceType.Iron:
                {
                    var index = Random.Range(0, MapController.Instance.metalTemplate.Count);
                    obj = GameObject.Instantiate(MapController.Instance.metalTemplate[index], this.transform);
                    this.objName = "����";
                    break;
                }
            default:
                {
                    obj = null;
                    break;
                }
        }
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale *= 2;
        obj.transform.eulerAngles = new Vector3(0, rot, 0);
        var res = this.gameObject.AddComponent<CompResource>();
        res.MaxHP = 10;
        res.HP = 10;
        res.MaxEP = 10;
        res.EP = 10;
        res.ResourceAcquireAmount = 10;
        res.resourceType = type;

        base.InitThis();
    }

    public override void OnBeingDestroyed()
    {
        
    }
    public override void OnInteracted()
    {
        foreach (var comp in components)
        {
            comp.OnApply(0);
        }
    }
}
