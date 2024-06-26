using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseObj : MonoBehaviour
{
    public Vector3Int Pos;
    public string ID;
    public MoveType moveType;
    public MoveStyle moveStyle;
    public string objName;
    public List<BaseComponent> components = new List<BaseComponent>();
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

    }
    public abstract void OnInteracted();
    public abstract void OnBeingDestroyed();
}
