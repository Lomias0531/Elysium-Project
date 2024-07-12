using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Singletion<CameraController>
{
    private Vector3 oldMousePosition;
    public float mapScaleSpeed = 5;
    public float minScale;
    public float maxScale;

    public GameObject obj_CameraFocusDummy;

    Tween CamMoveTween;

    public float keyboardTranslateSpeed = 5f;
    public float scrollRollSpeed = 1f;
    public float scrollSpeedX = 1f;
    public float scrollSpeedY = 1f;

    public float camYMin;
    public float camYMax;
    bool isRMBPresed = false;
    public float camAngleX = 0;
    float camAngleY = 60f;
    float camDistance = 10f;

    bool isFocusing = false;
    BaseTile curLookingTile;
    // Start is called before the first frame update
    void Start()
    {
        if (!obj_CameraFocusDummy)
        {
            obj_CameraFocusDummy = new GameObject("CamDummy");
            obj_CameraFocusDummy.transform.position = new Vector3(0, 0, 0);
        }
        DOTween.To(() => camAngleX, x => camAngleX = x, 180f, 0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        GetTileCurLookingAt();
        ApplyMidleMouseButtonMovementSpeed();
        GetKeyboardCamTranslate();
    }
    private void ApplyMidleMouseButtonMovementSpeed()
    {
        if(!isFocusing)
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

                obj_CameraFocusDummy.transform.position -= Tools.CalculateMousePosNormal(Camera.main, delta, distance);

                oldMousePosition = newMousePos;
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            camDistance += scrollRollSpeed * Time.deltaTime;

        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            camDistance -= scrollRollSpeed * Time.deltaTime;
        }
        camDistance = Mathf.Clamp(camDistance, minScale, maxScale);

        isRMBPresed = false;
        if(Input.GetMouseButton(1))
        {
            isRMBPresed = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if(Input.GetMouseButtonUp(1))
        {
            isRMBPresed = false;
            Cursor.lockState = CursorLockMode.None;
        }
        if(isRMBPresed)
        {
            camAngleX += Input.GetAxis("Mouse X") * Time.deltaTime * scrollSpeedX;
            camAngleY -= Input.GetAxis("Mouse Y") * Time.deltaTime * scrollSpeedY;

            camAngleY = ClampAngle(camAngleY, camYMin, camYMax);
        }
    }
    private void LateUpdate()
    {
        obj_CameraFocusDummy.transform.eulerAngles = new Vector3(0, camAngleX, 0);

        var horLength = camDistance * Mathf.Cos(Mathf.Deg2Rad * camAngleY);
        var height = camDistance * Mathf.Sin(Mathf.Deg2Rad * camAngleY);
        var camX = horLength * Mathf.Sin(Mathf.Deg2Rad * camAngleX);
        var camZ = horLength * Mathf.Cos(Mathf.Deg2Rad * camAngleX);
        this.transform.position = obj_CameraFocusDummy.transform.position + new Vector3(camX, height, camZ);

        this.transform.LookAt(obj_CameraFocusDummy.transform);
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
    public IEnumerator CamFocusOnTarget(BaseObj target)
    {
        if (CamMoveTween != null)
        {
            CamMoveTween.Kill();
        }

        if (target != null)
        {
            //isFocusing = true;
            CamMoveTween = obj_CameraFocusDummy.transform.DOMove(target.transform.position, 0.2f);
            yield return new WaitForSeconds(0.2f);
            obj_CameraFocusDummy.transform.SetParent(target.gameObject.transform);
        }else
        {
            obj_CameraFocusDummy.transform.SetParent(null);
            //CamMoveTween = obj_CameraFocusDummy.transform.DOMoveY(0f, 0.2f);
            isFocusing = false;
        }
    }
    void GetKeyboardCamTranslate()
    {
        if (isFocusing) return;
        if(Input.GetKey(KeyCode.W))
        {
            obj_CameraFocusDummy.transform.Translate(new Vector3(0, 0, Time.deltaTime * -keyboardTranslateSpeed), obj_CameraFocusDummy.transform);
        }
        if (Input.GetKey(KeyCode.S))
        {
            obj_CameraFocusDummy.transform.Translate(new Vector3(0, 0, Time.deltaTime * keyboardTranslateSpeed), obj_CameraFocusDummy.transform);
        }
        if (Input.GetKey(KeyCode.A))
        {
            obj_CameraFocusDummy.transform.Translate(new Vector3(Time.deltaTime * keyboardTranslateSpeed, 0, 0), obj_CameraFocusDummy.transform);
        }
        if (Input.GetKey(KeyCode.D))
        {
            obj_CameraFocusDummy.transform.Translate(new Vector3(Time.deltaTime * -keyboardTranslateSpeed, 0, 0), obj_CameraFocusDummy.transform);
        }
    }
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
    void GetTileCurLookingAt()
    {
        Ray ray = new Ray(new Vector3(obj_CameraFocusDummy.transform.position.x, 1000, obj_CameraFocusDummy.transform.position.z), Vector3.down);
        var result = Physics.RaycastAll(ray, Mathf.Infinity);
        if(result.Length > 0)
        {
            foreach (var hit in result)
            {
                var tile = hit.collider.gameObject.GetComponent<BaseTile>();
                if (tile != null)
                {
                    curLookingTile = tile;
                    break;
                }
            }
        }
        if(curLookingTile != null && !isFocusing)
        {
            var y = obj_CameraFocusDummy.transform.position.y + (curLookingTile.transform.position.y - obj_CameraFocusDummy.transform.position.y) * (Time.deltaTime / 0.2f);
            obj_CameraFocusDummy.transform.position = new Vector3(obj_CameraFocusDummy.transform.position.x, y, obj_CameraFocusDummy.transform.position.z);
        }
    }
    public void ResetViewPoint()
    {
        obj_CameraFocusDummy.transform.DOLocalMove(Vector3.zero, 0.1f);
    }
}
