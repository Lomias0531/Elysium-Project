using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 oldMousePosition;
    public float mapScaleSpeed = 5;
    public float minScale;
    public float maxScale;

    public GameObject obj_CameraFocusDummy;

    Tween CamMoveTween;

    [SerializeField]
    float camHeight;
    // Start is called before the first frame update
    void Start()
    {
        if (obj_CameraFocusDummy == null)
        {
            obj_CameraFocusDummy = new GameObject("CamDummy");
            obj_CameraFocusDummy.transform.position = new Vector3(0, 0, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ApplyMidleMouseButtonMovementSpeed();
    }
    private void ApplyMidleMouseButtonMovementSpeed()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            oldMousePosition = Input.mousePosition;
        }
        else if (Input.GetKey(KeyCode.Mouse2))
        {
            Vector3 newMousePos = Input.mousePosition;
            //获得一帧的相机移动
            var delta = newMousePos - oldMousePosition;

            var distance = 0f;

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                distance = Vector3.Distance(hit.point, transform.position);
            }

            obj_CameraFocusDummy.transform.position = obj_CameraFocusDummy.transform.position - Tools.CalculateMousePosNormal(Camera.main, delta, distance);

            oldMousePosition = newMousePos;
        }

        camHeight = this.transform.position.y;
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            camHeight += mapScaleSpeed * Time.deltaTime;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            camHeight -= mapScaleSpeed * Time.deltaTime;
        }

        camHeight = Mathf.Clamp(camHeight, minScale, maxScale);
    }
    private void LateUpdate()
    {
        this.transform.position = new Vector3(obj_CameraFocusDummy.transform.position.x, camHeight, obj_CameraFocusDummy.transform.position.z - camHeight * Mathf.Tan(60f));
    }
    public void MoveCamTo(Vector3Int pos)
    {
        if (CamMoveTween != null)
        {
            CamMoveTween.Kill();
        }

        Vector3 targetPos = Vector3.zero;
        if (MapController.Instance.mapTiles.ContainsKey(pos))
        {
            targetPos = MapController.Instance.mapTiles[pos].gameObject.transform.position;
        }
        CamMoveTween = obj_CameraFocusDummy.transform.DOMove(targetPos, 0.2f);
    }
}
