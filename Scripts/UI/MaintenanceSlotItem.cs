using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaintenanceSlotItem : MonoBehaviour
{
    public BaseUnitSpot thisSpot;
    public Button btn_TriggerFocus;
    public Text txt_SpotName;
    private void Start()
    {
        btn_TriggerFocus.onClick.AddListener(TriggerFocusToThisSpot);
    }
    public void InitThis(BaseUnitSpot spot)
    {
        thisSpot = spot;
        txt_SpotName.text = spot.thisSpotName;
        //Debug.Log(thisSpot.gameObject.transform.position);
    }
    void TriggerFocusToThisSpot()
    {
        CameraController.Instance.FocusCamToTarget(thisSpot.gameObject);
    }
}
