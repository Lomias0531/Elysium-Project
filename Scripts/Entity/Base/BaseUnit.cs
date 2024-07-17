using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : BaseObj
{
    //DO NOT DELETE THIS!
    //USE FOR ABSTRACT CLASS!
    bool lookAtCam = false;
    public override void OnBeingDestroyed()
    {
        
    }

    public override void OnInteracted()
    {
        
    }

    public override void OnSelected()
    {
        lookAtCam = true;
        StartCoroutine(SayHello());
    }
    IEnumerator SayHello()
    {
        if (animator != null)
        {
            animator.CrossFadeInFixedTime("Interact", 0.1f);
            yield return new WaitForSeconds(1f);
            animator.CrossFadeInFixedTime("Idle", 0.1f);
        }
    }

    public override void OnUnselected()
    {
        lookAtCam = false;
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
    private void OnAnimatorIK(int layerIndex)
    {
        if(lookAtCam && animator != null)
        {
            animator.SetLookAtPosition(Camera.main.transform.position);
            animator.SetLookAtWeight(1f);
        }else
        {
            animator.SetLookAtPosition(this.transform.position + this.transform.forward);
            animator.SetLookAtWeight(0f);
        }
    }
}
