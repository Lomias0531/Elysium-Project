using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;
using Random = UnityEngine.Random;
using static UnityEditor.Progress;

public abstract class BaseObj : MonoBehaviour
{
    public Vector3Int Pos;
    public string EntityID;
    public Animator animator;
    [HideInInspector]
    public MoveType[] moveType
    {
        get
        {
            List<MoveType> list = new List<MoveType>();
            foreach (var comp in components)
            {
                foreach (var item in comp.functions)
                {
                    if(item.function == BaseComponent.ComponentFunction.Mobile)
                    {
                        list.Add((MoveType)item.functionIntVal[0]);
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
                foreach (var item in comp.functions)
                {
                    if (item.function == BaseComponent.ComponentFunction.Mobile)
                    {
                        list.Add((MoveStyle)item.functionIntVal[1]);
                    }
                }
            }
            return list.ToArray();
        }
    }
    public string objName;
    [HideInInspector]
    List<BaseComponent> components = new List<BaseComponent>();
    public List<BaseComponent> Components
    {
        get
        {
            return components;
        }
    }
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
    public bool isUniderConstruction
    {
        get
        {
            var comp = GetDesiredComponent<CompConstructTemp>();
            return comp != null;
        }
    }

    [HideInInspector]
    public BaseComponent curSelectedComp;
    [HideInInspector]
    public CompFunctionDetail curSelectedFunction;
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
        this.EntityID = id.ToString();

        var _components = this.gameObject.GetComponents<BaseComponent>();
        components = _components.ToList();
        foreach (var item in components)
        {
            item.thisObj = this;
            item.LoadCompDataViaID();
        }
        animator = this.gameObject.GetComponent<Animator>();
    }
    public abstract void OnInteracted();
    public abstract void OnBeingDestroyed();
    public abstract void OnSelected();
    public abstract void OnUnselected();
    public T GetDesiredComponent<T>() where T:BaseComponent
    {
        foreach (var comp in components)
        {
            if(comp.GetType() == typeof(T))
            {
                return (T)comp;
            }
        }
        return null;
    }
    public T[] GetDesiredComponents<T>() where T:BaseComponent
    {
        List<BaseComponent> result = new List<BaseComponent>();
        foreach (var comp in components)
        {
            if (comp.GetType() == typeof(T))
            {
                result.Add(comp);
            }
        }
        return result.OfType<T>().ToArray();
    }
    public void RemoveDesiredComponent(BaseComponent comp)
    {
        if(components.Contains(comp))
        {
            components.Remove(comp);
            Destroy(comp);
        }
    }

    public void TakeDamage(float damageValue, CompWeapon.WeaponAttackType damageType)
    {
        switch(damageType)
        {
            default:
                {
                    break;
                }
            case CompWeapon.WeaponAttackType.Pierce:
                {
                    Dictionary<int, int> dic = new Dictionary<int, int>();
                    int index = 0;
                    for (int i = 0; i < Components.Count; i++)
                    {
                        for (int t = 0; t < Components[i].HP; t++)
                        {
                            dic.Add(index, i);
                            index += 1;
                        }
                    }

                    int compIndex = Random.Range(0, dic.Count);
                    var damagedComp = Components[dic[compIndex]];

                    damagedComp.HP -= damageValue - damagedComp.Defense;
                    if (damagedComp.HP <= 0)
                    {
                        damagedComp.HP = 0;
                        Debug.Log("Component destroyed");
                        Components.Remove(damagedComp);

                        if(damagedComp.isCritical)
                        {
                            Debug.Log("Critical component lost, unit destroyed");
                            MapController.Instance.RemoveObject(this);
                        }

                        if(Components.Count <= 0)
                        {
                            Debug.Log("All components lost, unit destroyed");
                            MapController.Instance.RemoveObject(this);
                        }

                        Destroy(damagedComp);
                    }
                    break;
                }
            case CompWeapon.WeaponAttackType.Blast:
                {
                    do
                    {
                        var dam = Random.Range(1f, damageValue);
                        Dictionary<int, int> dic = new Dictionary<int, int>();
                        int index = 0;
                        for (int i = 0; i < Components.Count; i++)
                        {
                            for (int t = 0; t < Components[i].HP; t++)
                            {
                                dic.Add(index, i);
                                index += 1;
                            }
                        }

                        int compIndex = Random.Range(0, dic.Count);
                        var damagedComp = Components[dic[compIndex]];

                        damagedComp.HP -= dam - damagedComp.Defense;
                        damageValue -= dam;
                        if (damagedComp.HP <= 0)
                        {
                            damagedComp.HP = 0;
                            Debug.Log("Component destroyed");
                            Components.Remove(damagedComp);

                            if (damagedComp.isCritical)
                            {
                                Debug.Log("Critical component lost, unit destroyed");
                                MapController.Instance.RemoveObject(this);
                            }

                            if (Components.Count <= 0)
                            {
                                Debug.Log("All components lost, unit destroyed");
                                MapController.Instance.RemoveObject(this);
                            }

                            Destroy(damagedComp);
                        }
                    } while (damageValue > 0);
                    break;
                }
        }
    }
}
