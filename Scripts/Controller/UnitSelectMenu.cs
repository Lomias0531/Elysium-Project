using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectMenu : MonoBehaviour
{
    BaseObj selectedObj;
    Coroutine expandRoutine;
    bool expandFinished = true;

    float expandTimeElapsed;
    public Image img_MP;
    public Image img_HP;

    public CompSkillTrigger skillTrigger;
    public Transform tsf_SkillTriggerContainer;
    List<CompSkillTrigger> skillTriggers = new List<CompSkillTrigger>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelectedObjectStatus();
        this.transform.eulerAngles = Vector3.zero;
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
            this.transform.SetParent(null);
        }else
        {
            selectedObj = thisUnit;
            //this.gameObject.transform.position = thisUnit.transform.position;
            this.transform.SetParent(thisUnit.transform);
            this.transform.localPosition = Vector3.zero;

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

        if(selectedObj.Faction == "Elysium")
        {
            foreach (var comp in selectedObj.components)
            {
                for (int i = 0; i < comp.functions.Length; i++)
                {
                    var trigger = GameObject.Instantiate(skillTrigger, tsf_SkillTriggerContainer);
                    trigger.gameObject.SetActive(true);
                    skillTriggers.Add(trigger);
                    trigger.InitThis(true, this, i, comp);
                }
            }
        }

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
    void SetUIDegrees(float val)
    {
        var HPDiv = selectedObj.HP / selectedObj.HPMax;
        var MPDiv = selectedObj.EP / selectedObj.EPMax;

        if (val <= HPDiv)
        {
            img_HP.fillAmount = val;
        }
        else
        {
            img_HP.fillAmount = HPDiv;
        }
        if (val <= MPDiv)
        {
            img_MP.fillAmount = val;
        }
        else
        {
            img_MP.fillAmount = MPDiv;
        }

        if (skillTriggers.Count > 0)
        {
            for (int i = 0; i < skillTriggers.Count; i++)
            {
                var posX = Mathf.Sin(360f * Mathf.Deg2Rad * val * ((float)i / (float)skillTriggers.Count)) * 0.75f;
                var posY = Mathf.Cos(360f * Mathf.Deg2Rad * val * ((float)i / (float)skillTriggers.Count)) * 0.75f;
                skillTriggers[i].transform.localPosition = new Vector3(posX, posY, 0);
            }
        }
    }
    void UpdateSelectedObjectStatus()
    {
        if (selectedObj == null) return;
        if(expandFinished)
        {
            img_HP.fillAmount = selectedObj.HP / selectedObj.HPMax;
            img_MP.fillAmount = selectedObj.EP / selectedObj.EPMax;
        }
    }
}
