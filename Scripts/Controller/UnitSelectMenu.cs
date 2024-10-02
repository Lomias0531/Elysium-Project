using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectMenu : MonoBehaviour
{
    BaseObj selectedObj;
    Coroutine expandRoutine;
    bool expandFinished = true;
    bool isTracking = false;

    float expandTimeElapsed;
    public Image img_MP;
    public Image img_HP;

    public CompSkillTrigger skillTrigger;
    public CompItemTrigger itemTrigger;
    public Transform tsf_SkillTriggerContainer;
    List<BaseCompTrigger> skillTriggers = new List<BaseCompTrigger>();
    Dictionary<BaseComponent,Image> HPBars = new Dictionary<BaseComponent,Image>();
    Dictionary<BaseComponent,Image> MPBars = new Dictionary<BaseComponent,Image>();
    public Transform tsf_BarContainer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelectedObjectStatus();

        var target = CameraController.Instance.obj_CameraFocusDummy.transform.eulerAngles.y;
        var self = this.transform.eulerAngles.y;
        if(target - self > 180)
        {
            target -= 360;
        }
        if(target - self < -180)
        {
            self -= 360;
        }

        var angle = (target - self) * (Time.deltaTime / 0.2f);

        if (angle > 180) angle -= 180;
        if (angle < -180) angle += 180;

        //if(Mathf.Abs(angle) > 10)
        //{
        //    Debug.Log(target + " " + self);
        //}
        if(selectedObj != null)
        {
            this.transform.position = selectedObj.transform.position;
            isTracking = true;
        }else
        {
            if(isTracking)
            {
                expandTimeElapsed = 0;
                StartCoroutine(RetractThis());

                isTracking = false;
            }
        }

        this.transform.Rotate(new Vector3(0, angle, 0));
    }
    public void OnSelectUnit(BaseObj thisUnit)
    {
        if (thisUnit == selectedObj) return;

        if (expandRoutine != null)
        {
            StopCoroutine(expandRoutine);
        }

        expandTimeElapsed = 0;

        if (thisUnit == null)
        {
            expandRoutine = StartCoroutine(RetractThis());
            //this.transform.SetParent(null);
        }else
        {
            selectedObj = thisUnit;

            foreach (var item in HPBars)
            {
                Destroy(item.Value.gameObject);
            }
            foreach (var item in MPBars)
            {
                Destroy(item.Value.gameObject);
            }
            HPBars.Clear();
            MPBars.Clear();

            //this.transform.SetParent(thisUnit.transform);
            this.transform.localPosition = Vector3.zero;

            for(int i =0;i<selectedObj.Components.Count;i++)
            {
                var hpBar = GameObject.Instantiate(img_HP, tsf_BarContainer);
                float hpVal = 0;
                for(int t = 0;t<i;t++)
                {
                    hpVal += selectedObj.Components[t].MaxHP;
                }
                float thisHPAng = 180f * hpVal / selectedObj.HPMax;
                hpBar.gameObject.SetActive(true);
                hpBar.transform.localEulerAngles = new Vector3(0, 0, 180f + thisHPAng);

                var mpBar = GameObject.Instantiate(img_MP, tsf_BarContainer);
                float mpVal = 0;
                for(int t = 0; t<i;t++)
                {
                    mpVal += selectedObj.Components[t].MaxEP;
                }
                float thisMPAng = 180f * mpVal / selectedObj.EPMax;
                mpBar.gameObject.SetActive(true);
                mpBar.transform.localEulerAngles = new Vector3(0, 0, -thisMPAng);

                HPBars.Add(selectedObj.Components[i], hpBar);
                MPBars.Add(selectedObj.Components[i], mpBar);
            }

            if (selectedObj.Faction == "Elysium")
            {
                foreach (var comp in selectedObj.Components)
                {
                    if (comp.thisCompData.functions == null) continue;
                    for (int i = 0; i < comp.thisCompData.functions.Length; i++)
                    {
                        var trigger = GameObject.Instantiate(skillTrigger, tsf_SkillTriggerContainer);
                        trigger.gameObject.SetActive(true);
                        skillTriggers.Add(trigger);
                        trigger.InitThis(true, i, comp, this);
                    }
                }
            }

            expandRoutine = StartCoroutine(ExpandThis());
        }
    }
    public IEnumerator RetractThis()
    {
        expandFinished = false;

        do
        {
            float div = 1f - expandTimeElapsed / 0.2f;

            SetUIDegrees(div);

            expandTimeElapsed += Time.deltaTime;
            yield return null;

        } while (expandTimeElapsed <= 0.2f);

        SetUIDegrees(0f);

        selectedObj = null;

        foreach (var trigger in skillTriggers)
        {
            Destroy(trigger.gameObject);
        }
        skillTriggers.Clear();

        expandFinished = true;
    }
    public IEnumerator ExpandThis()
    {
        expandFinished = false;

        do
        {
            float div = expandTimeElapsed / 0.2f;

            SetUIDegrees(div);

            expandTimeElapsed += Time.deltaTime;
            yield return null;

        } while (expandTimeElapsed <= 0.2f);

        SetUIDegrees(1f);
        expandFinished = true;
    }
    public IEnumerator ShowEntityInventory()
    {
        float iconExpandTime = 0f;
        do
        {
            var val = 1f - iconExpandTime / 0.2f;
            SetIconDegrees(val);
            iconExpandTime += Time.deltaTime;
            yield return null;

        } while (iconExpandTime <= 0.2f);
        foreach (var trigger in skillTriggers)
        {
            Destroy(trigger.gameObject);
        }
        skillTriggers.Clear();
        var inv = selectedObj.GetDesiredComponent<CompStorage>();
        if(inv != null)
        {
            for(int i = 0;i<inv.inventory.Count;i++)
            {
                var trigger = GameObject.Instantiate(itemTrigger, tsf_SkillTriggerContainer);
                trigger.gameObject.SetActive(true);
                skillTriggers.Add(trigger);
                trigger.InitThis(inv, i, this);
            }
        }
        iconExpandTime = 0f;
        do
        {
            var val = iconExpandTime / 0.2f;
            SetIconDegrees(val);
            iconExpandTime += Time.deltaTime;
            yield return null;
        } while (iconExpandTime <= 0.2f);
    }
    void SetUIDegrees(float val)
    {
        foreach (var item in HPBars)
        {
            if(item.Key == null)
            {
                item.Value.fillAmount = 0;
            }
            else
            {
                float fullDiv;
                float HPDiv;
                if(selectedObj != null)
                {
                    fullDiv = item.Key.MaxHP / selectedObj.HPMax;
                    HPDiv = item.Key.HP / item.Key.MaxHP;
                }
                else
                {
                    fullDiv = 1;
                    HPDiv = item.Value.fillAmount;
                }
                if (val <= HPDiv)
                {
                    item.Value.fillAmount = val * fullDiv;
                }
                else
                {
                    item.Value.fillAmount = HPDiv * fullDiv;
                }
            }
        }
        foreach (var item in MPBars)
        {
            if (item.Key == null)
            {
                item.Value.fillAmount = 0;
            }
            else
            {
                float fullDiv;
                float MPDiv;
                if(selectedObj != null)
                {
                    fullDiv = item.Key.MaxEP / selectedObj.EPMax;
                    MPDiv = item.Key.EP / item.Key.MaxEP;
                }
                else
                {
                    fullDiv = 1;
                    MPDiv = item.Value.fillAmount;
                }
                if (val <= MPDiv)
                {
                    item.Value.fillAmount = val * fullDiv;
                }
                else
                {
                    item.Value.fillAmount = MPDiv * fullDiv;
                }
            }
        }
        SetIconDegrees(val);
    }
    void SetIconDegrees(float val)
    {
        if (skillTriggers.Count > 0)
        {
            for (int i = 0; i < skillTriggers.Count; i++)
            {
                var posX = Mathf.Sin(360f * Mathf.Deg2Rad * val * ((float)i / (float)skillTriggers.Count)) * 0.75f;
                var posY = Mathf.Cos(360f * Mathf.Deg2Rad * val * ((float)i / (float)skillTriggers.Count)) * 0.75f;
                skillTriggers[i].transform.localPosition = new Vector3(posX, posY, 0);

                //var isLeft = i % 2 == 0 ? 1 : -1;
                //var arrange = Mathf.FloorToInt(i / 2);

                //var posX = (0.75f + arrange * val * 0.35f) * isLeft;
                //skillTriggers[i].transform.localPosition = new Vector3(posX, 0, 0);
            }
        }
    }
    void UpdateSelectedObjectStatus()
    {
        if (selectedObj == null) return;
        if(expandFinished)
        {
            SetUIDegrees(1f);
        }
    }
    public void RemoveTrigger()
    {
        StartCoroutine(ShowEntityInventory());
    }
    public void HoveringComponent(BaseComponent comp)
    {
        foreach (var item in HPBars)
        {
            if(item.Key == comp)
            {
                item.Value.transform.DOLocalMoveZ(-0.1f, 0.2f);
            }else
            {
                item.Value.transform.DOLocalMoveZ(0f, 0.2f);
            }
        }
        foreach (var item in MPBars)
        {
            if (item.Key == comp)
            {
                item.Value.transform.DOLocalMoveZ(-0.1f, 0.2f);
            }
            else
            {
                item.Value.transform.DOLocalMoveZ(0f, 0.2f);
            }
        }
    }
}
