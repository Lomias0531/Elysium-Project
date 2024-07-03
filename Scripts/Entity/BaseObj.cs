using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public abstract class BaseObj : MonoBehaviour
{
    public Vector3Int Pos;
    public string ID;
    [HideInInspector]
    public MoveType[] moveType
    {
        get
        {
            List<MoveType> list = new List<MoveType>();
            foreach (var comp in components)
            {
                if(comp.GetType() == typeof(CompMobile))
                {
                    foreach (var item in comp.functions)
                    {
                        list.Add((BaseUnit.MoveType)item.functionIntVal[0]);
                    }
                }
            }
            return list.ToArray();
        }
    }
    [HideInInspector]
    public MoveStyle[] moveStyle
    {
        get
        {
            List<MoveStyle> list = new List<MoveStyle>();
            foreach (var comp in components)
            {
                if (comp.GetType() == typeof(CompMobile))
                {
                    foreach (var item in comp.functions)
                    {
                        list.Add((BaseUnit.MoveStyle)item.functionIntVal[1]);
                    }
                }
            }
            return list.ToArray();
        }
    }
    public string objName;
    [HideInInspector]
    public List<BaseComponent> components = new List<BaseComponent>();
    public string Faction;
    public float HP
    {
        get
        {
            float value = 0f;
            foreach (var item in components)
            {
                value += item.HP;
            }
            return value;
        }
    }
    public float HPMax
    {
        get
        {
            float value = 0f;
            foreach (var item in components)
            {
                value += item.MaxHP;
            }
            return value;
        }
    }
    public float EP
    {
        get
        {
            float value = 0f;
            foreach (var item in components)
            {
                value += item.EP;
            }
            return value;
        }
    }
    public float EPMax
    {
        get
        {
            float value = 0f;
            foreach (var item in components)
            {
                value += item.MaxEP;
            }
            return value;
        }
    }

    public enum MoveType
    {
        None,
        Ground,
        Water,
        Air,
    }
    public enum MoveStyle
    {
        None,
        Ordinary,
        Jump,
        Teleport,
    }

    public MoveType curSelectedMoveType;
    public MoveStyle curSelectedMoveStyle;
    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }
    public virtual void InitThis()
    {
        Guid id = Guid.NewGuid();
        this.ID = id.ToString();

        var _components = this.gameObject.GetComponents<BaseComponent>();
        components = _components.ToList();
        foreach (var item in components)
        {
            item.thisObj = this;
        }
    }
    public abstract void OnInteracted();
    public abstract void OnBeingDestroyed();

    public IEnumerator MoveObjectToTile(BaseTile tile)
    {
        var moveQueue = this.UnitFindPath(tile, this.curSelectedMoveType);

        do
        {
            var target = moveQueue.Dequeue();
            this.transform.DOMove(target.transform.position, 0.2f);
            this.Pos = target.Pos;
            yield return new WaitForSeconds(0.2f);
        }while(moveQueue.Count > 0);
        PlayerController.Instance.CancelAllOperations();
    }
}
