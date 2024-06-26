using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UnitSelectMenu : MonoBehaviour
{
    BaseObj selectedObj;
    Coroutine expandRoutine;

    float expandTimeElapsed;
    public Image img_Circle;
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

        if (expandRoutine != null)
        {
            StopCoroutine(expandRoutine);
        }

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
        yield return null;
    }
    IEnumerator ExpandThis()
    {
        yield return null;
    }
}
