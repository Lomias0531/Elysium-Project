using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
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
    public BaseObj focusingTarget;

    Vector3 lastDummyPos;
    Vector3 lastDummyRot;

    public Camera cam_Focus;
    public Camera cam_Origin;
    GameObject FocusTarget;
    Vector3 focusOriginPos;
    public GameObject obj_Maintenance;

    public Vector3 sphereCenter = new Vector3(1000, 1000, 1000); // �������ĵ�λ��  
    public float sphereRadius = 10.0f; // ����뾶  
    public float movementSpeed = 1.0f; // �ƶ��ٶ�  

    private Vector3 startDirection;
    private Vector3 endDirection;
    private float movementProgress = 0.0f;
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
        if (obj_CameraFocusDummy == null)
        {
            obj_CameraFocusDummy = new GameObject("CamDummy");
            obj_CameraFocusDummy.transform.position = lastDummyPos;
            obj_CameraFocusDummy.transform.eulerAngles = lastDummyRot;
        }
        obj_CameraFocusDummy.transform.eulerAngles = new Vector3(0, camAngleX, 0);

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
                //���һ֡������ƶ�
                var delta = newMousePos - oldMousePosition;

                var distance = 0f;

                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
                {
                    distance = Vector3.Distance(hit.point, cam_Origin.transform.position);
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
        if(obj_CameraFocusDummy == null)
        {
            obj_CameraFocusDummy = new GameObject("CamDummy");
            obj_CameraFocusDummy.transform.position = lastDummyPos;
            obj_CameraFocusDummy.transform.eulerAngles = lastDummyRot;
        }
        obj_CameraFocusDummy.transform.eulerAngles = new Vector3(0, camAngleX, 0);

        var horLength = camDistance * Mathf.Cos(Mathf.Deg2Rad * camAngleY);
        var height = camDistance * Mathf.Sin(Mathf.Deg2Rad * camAngleY);
        var camX = horLength * Mathf.Sin(Mathf.Deg2Rad * camAngleX);
        var camZ = horLength * Mathf.Cos(Mathf.Deg2Rad * camAngleX);
        cam_Origin.transform.position = obj_CameraFocusDummy.transform.position + new Vector3(camX, height, camZ);

        cam_Origin.transform.LookAt(obj_CameraFocusDummy.transform);

        lastDummyPos = obj_CameraFocusDummy.transform.position;
        lastDummyRot = obj_CameraFocusDummy.transform.eulerAngles;
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

        focusingTarget = target;

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
        curLookingTile = Tools.GetTileViaCoord(obj_CameraFocusDummy.transform.position);
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
    public void InitFocus()
    {
        obj_Maintenance.SetActive(true);
        cam_Origin.gameObject.SetActive(false);
    }
    public void FocusCamToTarget(GameObject target)
    {
        FocusTarget = target;
        focusOriginPos = cam_Focus.gameObject.transform.position;
        // ������������ĵ�A���B��ķ�������  
        startDirection = (focusOriginPos - sphereCenter).normalized * sphereRadius;
        //endDirection = (FocusTarget.transform.position - sphereCenter).normalized * sphereRadius;
        var meshes = PlayerController.Instance.FocusedUnit.GetComponentInChildren<SkinnedMeshRenderer>();
        var center = meshes.bounds.center;
        var pos = center - FocusTarget.transform.position;
        pos.x *= 10;
        pos.z *= 10;
        var dir = pos.normalized;
        endDirection = -dir * sphereRadius;

        // ���������ʼλ������ΪA���Ӧ������λ��  
        cam_Focus.gameObject.transform.position = sphereCenter + startDirection;

        // ��ʼ�����������Ϊע��B�㣨�����ǵ���������ƫ�ƣ���Ҫ������Է���  
        Vector3 lookDirection = FocusTarget.transform.position - cam_Focus.transform.position;
        lookDirection.Normalize(); // ȷ�����������ǵ�λ����  
        cam_Focus.gameObject.transform.forward = lookDirection; // ע�⣺����ʹ����forward������LookAt����ΪLookAt��ı��������up����  

        StartCoroutine(MoveFocusCam());
    }
    IEnumerator MoveFocusCam()
    {
        do
        {
            // �����ֵ����������ʱ��������߼���  
            movementProgress += Time.deltaTime * movementSpeed;
            movementProgress = Mathf.Clamp01(movementProgress); // ȷ����0��1֮��  

            // ʹ��Slerp�����ֵ��  
            Vector3 interpolatedPosition = sphereCenter + Vector3.Slerp(startDirection, endDirection, movementProgress);

            // ���������λ��  
            cam_Focus.gameObject.transform.position = interpolatedPosition;

            // �������������Ϊע��B�㣨���ǵ���������ƫ�ƣ�  
            Vector3 lookDirection = FocusTarget.transform.position - cam_Focus.transform.position;
            lookDirection.Normalize(); // ȷ�����������ǵ�λ����  
            cam_Focus.gameObject.transform.forward = lookDirection; // �����������ǰ��Ϊ����B��ķ���  

            // ����ﵽ�յ㣬�������û�ִ�������߼�  
            if (movementProgress >= 1.0f)
            {
                movementProgress = 0.0f; // ���ý��ȣ�������������߼�  
                break;
            }
            yield return null;
        } while (true);
    }
}
