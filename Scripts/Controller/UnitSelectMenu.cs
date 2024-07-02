using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectMenu : MonoBehaviour
{
    BaseObj selectedObj;
    Coroutine expandRoutine;

    float expandTimeElapsed;
    public Image img_Circle;

    public CompSkillTrigger skillTrigger;
    public Transform tsf_SkillTriggerContainer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnSelectUnit(BaseObj thisUnit)
    {
        if (thisUnit == selectedObj) return;
        selectedObj = thisUnit;
        if(selectedObj != null)
            this.gameObject.transform.position = thisUnit.transform.position;

        if (expandRoutine != null)
        {
            StopCoroutine(expandRoutine);
        }

        expandTimeElapsed = 0;

        if (selectedObj == null)
        {
            expandRoutine = StartCoroutine(RetractThis());
        }else
        {
            expandRoutine = StartCoroutine(ExpandThis());
        }
    }
    IEnumerator RetractThis()
    {
        do
        {
            float div = expandTimeElapsed / 0.2f;
            img_Circle.fillAmount = 1f - div;

            expandTimeElapsed += Time.deltaTime;
            yield return null;

        } while (expandTimeElapsed <= 0.2f);

        img_Circle.fillAmount = 0f;
    }
    IEnumerator ExpandThis()
    {
        do
        {
            float div = expandTimeElapsed / 0.2f;
            img_Circle.fillAmount = div;

            expandTimeElapsed += Time.deltaTime;
            yield return null;

        } while (expandTimeElapsed <= 0.2f);

        img_Circle.fillAmount = 1f;
    }
    public void ShowDescription(string desc)
    {

    }
}
