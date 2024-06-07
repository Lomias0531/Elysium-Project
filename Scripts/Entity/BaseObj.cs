using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseObj : MonoBehaviour
{
    public Vector3Int Pos;
    public string ID;
    public MoveType moveType;
    public MoveStyle moveStyle;
    public List<BaseComponent> components = new List<BaseComponent>();

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
    public abstract void OnInteracted();
}
