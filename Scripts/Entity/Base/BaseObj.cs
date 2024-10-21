using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class BaseObj : MonoBehaviour
{
    public Vector3Int Pos;
    public string EntityID;
    public Animator animator;

    public BaseTile? curTile;

    public EntityData thisEntityData;

    public Transform tsf_Turret;
    public float turretTurnRate;

    public int maxStorageSlot;
    public List<ItemData> inventory = new List<ItemData>();

    public List<BaseUnitSpot> componentBasements = new List<BaseUnitSpot>();

    public GameObject WallBottomPoint;
    public GameObject WallTopPoint;

    public bool isAimedAtTarget = false;
    public BaseTile targetTile;

    float recoilTime;
    float recoilRadius;

    [HideInInspector]
    public MoveType[] moveType
    {
        get
        {
            List<MoveType> list = new List<MoveType>();
            var mobile = GetFunctionComponents(ComponentFunctionType.Mobile);
            foreach (var comp in mobile)
            {
                foreach (var item in comp.thisCompData.functions)
                {
                    if(item.functionIntVal == null) continue;
                    list.Add((MoveType)item.functionIntVal[0]);
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
            var mobile = GetFunctionComponents(ComponentFunctionType.Mobile);
            foreach (var comp in mobile)
            {
                foreach (var item in comp.thisCompData.functions)
                {
                    if (item.functionIntVal == null) continue;
                    list.Add((MoveStyle)item.functionIntVal[1]);
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
        AimAtTarget();

        if(tsf_Turret != null)
        {
            if(recoilTime > 0)
            {
                recoilTime -= Time.deltaTime;
                tsf_Turret.localPosition = tsf_Turret.forward * recoilRadius * recoilTime;
            }else
            {
                recoilTime = 0;
                tsf_Turret.localPosition = Vector3.zero;
            }
        }
    }
    public virtual void InitThis()
    {
        Guid id = Guid.NewGuid();
        this.EntityID = id.ToString();

        var attachments = this.gameObject.GetComponentsInChildren<BaseUnitSpot>();
        componentBasements = attachments.ToList();

        maxStorageSlot = thisEntityData.MaxInventoryCount;
        turretTurnRate = thisEntityData.TurretTurnRate;

        components = new List<BaseComponent>();

        var comp1 = this.gameObject.GetComponents<BaseComponent>();
        foreach ( var component in comp1)
        {
            components.Add(component);
        }

        if(thisEntityData.InstalledComponents != null)
        {
            for(int i = 0;i<thisEntityData.InstalledComponents.Length;i++)
            {
                Transform tsf_Target = null;
                bool isAvailable = true;
                foreach (var basement in componentBasements)
                {
                    if (basement.spotKey == thisEntityData.InstalledComponentsKey[i])
                    {
                        if(basement.InstalledCompCount < basement.MaxInstalledComp)
                        {
                            tsf_Target = basement.gameObject.transform;
                            basement.InstalledCompCount += 1;
                        }else
                        {
                            isAvailable = false;
                        }
                    }
                }
                if (!isAvailable) continue;

                var compData = DataController.Instance.GetComponentData(thisEntityData.InstalledComponents[i]);

                switch (compData.thisCompType)
                {
                    default:
                        {
                            break;
                        }
                    case CompType.Function:
                        {
                            CompFunction comp = this.gameObject.AddComponent<CompFunction>();
                            comp.thisCompData = compData;
                            comp.InitThis();
                            components.Add(comp);
                            comp.thisObj = this;
                            comp.tsf_InstalledSlot = tsf_Target;
                            break;
                        }
                    case CompType.Base:
                        {
                            CompBase comp = this.gameObject.AddComponent<CompBase>();
                            comp.thisCompData = compData;
                            comp.InitThis();
                            components.Add(comp);
                            comp.thisObj = this;
                            comp.tsf_InstalledSlot = tsf_Target;
                            break;
                        }
                    case CompType.WallConnector:
                        {
                            CompWallConnector comp = this.gameObject.AddComponent<CompWallConnector>();
                            comp.thisCompData = compData;
                            comp.InitThis();
                            components.Add(comp);
                            comp.thisObj = this;
                            comp.tsf_InstalledSlot = tsf_Target;
                            break;
                        }
                    case CompType.AutoController:
                        {
                            CompAutoController comp = this.gameObject.AddComponent<CompAutoController>();
                            comp.thisCompData = compData;
                            comp.InitThis();
                            components.Add(comp);
                            comp.thisObj = this;
                            comp.tsf_InstalledSlot = tsf_Target;
                            break;
                        }
                }
            }
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
    public CompFunction GetFunctionComponent(ComponentFunctionType type)
    {
        foreach (var comp in components)
        {
            if (comp.thisCompData.functions == null) continue;
            foreach (var func in comp.thisCompData.functions)
            {
                if(func.functionType == type)
                {
                    return (CompFunction)comp;
                }
            }
        }
        return null;
    }
    public List<CompFunction> GetFunctionComponents(ComponentFunctionType type)
    {
        List<CompFunction> result = new List<CompFunction>();
        foreach (var comp in components)
        {
            foreach (var func in comp.thisCompData.functions)
            {
                if (func.functionType == type)
                {
                    result.Add((CompFunction)comp);
                }
            }
        }
        return result;
    }
    public CompFunction GetFunctionComponent(CompType type)
    {
        foreach (var comp in components)
        {
            if (comp.thisCompData.functions == null) continue;
            if (comp.thisCompData.thisCompType == type) return (CompFunction)comp;
        }
        return null;
    }
    public List<CompFunction> GetFunctionComponents(CompType type)
    {
        List<CompFunction> result = new List<CompFunction>();
        foreach (var comp in components)
        {
            if(comp.thisCompData.thisCompType == type) result.Add((CompFunction)comp);
        }
        return result;
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

                    var dam = damageValue - damagedComp.thisCompData.ComponentDefense;
                    dam = dam <= 0 ? 1 : dam;
                    damagedComp.HP -= dam;
                    if (damagedComp.HP <= 0)
                    {
                        damagedComp.HP = 0;
                        Debug.Log("Component destroyed");
                        Components.Remove(damagedComp);

                        if(damagedComp.thisCompData.isFatalComponent)
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

                        if (index <= 0) return;

                        int compIndex = Random.Range(0, dic.Count);
                        var damagedComp = Components[dic[compIndex]];

                        var damage = dam - damagedComp.thisCompData.ComponentDefense;
                        damage = damage < 0 ? 1 : damage;
                        damagedComp.HP -= damage;
                        damageValue -= dam;
                        if (damagedComp.HP <= 0)
                        {
                            damagedComp.HP = 0;
                            Debug.Log("Component destroyed");
                            Components.Remove(damagedComp);

                            if (damagedComp.thisCompData.isFatalComponent)
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
    #region Storage
    public ItemData ReceiveItem(ItemData receivedItem)
    {
        //SO_ItemData itemInfo = DataController.Instance.GetItemInfo(receivedItem.itemID);
        var itemData = DataController.Instance.GetItemData(receivedItem.itemID);

        int index = 0;
        do
        {
            if (index >= inventory.Count && index < maxStorageSlot)
            {
                var rec = new ItemData();
                rec.itemID = receivedItem.itemID;
                rec.stackCount = 0;
                inventory.Add(rec);
            }

            if (index < inventory.Count)
            {
                if (inventory[index].itemID == receivedItem.itemID)
                {
                    if (inventory[index].stackCount + receivedItem.stackCount <= itemData.maxStackCount)
                    {
                        SetInvCount(index, inventory[index].stackCount + receivedItem.stackCount);
                        receivedItem.stackCount = 0;
                    }
                    else
                    {
                        var stackDiv = itemData.maxStackCount - inventory[index].stackCount;
                        SetInvCount(index, itemData.maxStackCount);
                        receivedItem.stackCount -= stackDiv;
                    }
                }
            }

            index++;
        } while (receivedItem.stackCount > 0 && index <= maxStorageSlot);

        return receivedItem;
    }
    public ItemData TransferItem(CompStorage targetStorage, ItemData transferedItem)
    {
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].itemID == transferedItem.itemID)
            {
                if (inventory[i].stackCount <= transferedItem.stackCount)
                {
                    ItemData dataTemp = new ItemData();
                    dataTemp.itemID = inventory[i].itemID;
                    dataTemp.stackCount = inventory[i].stackCount;

                    var itemData = targetStorage.ReceiveItem(dataTemp);
                    transferedItem.stackCount -= inventory[i].stackCount;
                    if (itemData.stackCount <= 0)
                    {
                        SetInvCount(i, 0);
                    }
                    else
                    {
                        SetInvCount(i, itemData.stackCount);
                    }
                }
                else
                {
                    var itemCount = transferedItem.stackCount;
                    var itemData = targetStorage.ReceiveItem(transferedItem);
                    var itemTransfered = itemCount - itemData.stackCount;

                    transferedItem.stackCount -= itemTransfered;
                    SetInvCount(i, inventory[i].stackCount - itemTransfered);
                }
            }
            if (inventory[i].stackCount <= 0)
            {
                inventory.RemoveAt(i);
            }
            if (transferedItem.stackCount <= 0) break;
        }
        return transferedItem;
    }
    public int GetItemCount(string itemID)
    {
        int result = 0;
        foreach (var item in inventory)
        {
            if (item.itemID == itemID)
            {
                result += item.stackCount;
            }
        }
        return result;
    }
    public void RemoveItem(ItemData itemInfo)
    {
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].itemID == itemInfo.itemID)
            {
                if (inventory[i].stackCount <= itemInfo.stackCount)
                {
                    ItemData dataTemp = new ItemData();
                    dataTemp.itemID = inventory[i].itemID;
                    dataTemp.stackCount = inventory[i].stackCount;

                    itemInfo.stackCount -= inventory[i].stackCount;
                    SetInvCount(i, 0);
                }
                else
                {
                    SetInvCount(i, inventory[i].stackCount - itemInfo.stackCount);
                    itemInfo.stackCount = 0;
                }
            }
            if (inventory[i].stackCount <= 0)
            {
                inventory.RemoveAt(i);
            }
            if (itemInfo.stackCount <= 0) break;
        }
    }
    void SetInvCount(int index, int count)
    {
        ItemData temp = new ItemData();
        temp.itemID = inventory[index].itemID;
        temp.stackCount = count;
        inventory[index] = temp;
    }
    #endregion
    void AimAtTarget()
    {
        if (targetTile == null) return;
        if(tsf_Turret == null)
        {
            isAimedAtTarget = true;
            return;
        }else
        {
            isAimedAtTarget = false;

            var dir = this.gameObject.transform.position - targetTile.gameObject.transform.position;
            dir.y = 0;

            float angle = Vector3.SignedAngle(tsf_Turret.forward, dir, Vector3.up);
            float rotateDir = Mathf.Sign(angle);

            float step;

            if (Mathf.Abs(angle) >= turretTurnRate * Time.deltaTime)
            {
                step = rotateDir * turretTurnRate * Time.deltaTime;

                tsf_Turret.Rotate(0, step, 0);
                return;
            }
            else
            {
                step = angle;

                tsf_Turret.Rotate(0, step, 0);

                isAimedAtTarget = true;
            }
        }
    }
    public void SetTarget(BaseTile target)
    {
        targetTile = target;
    }
    public void Recoil()
    {
        recoilTime = 0.3f;
        recoilRadius = 0.5f;
    }
}
