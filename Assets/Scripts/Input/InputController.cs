using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class InputController : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private float camSpeed = 10f;
    [SerializeField] private float maxTapTime = 0.2f;
    [SerializeField] private float maxDistanceForMovedFlag = 0.2f;
    [Header("Zoom")]
    [SerializeField] private float zoomMult = 0.01f;
    [SerializeField] private float zoomMin = 3f;
    [SerializeField] private float zoomMax = 8f;
    [SerializeField] private float wheelMouseZoomMult = 8f;
    [Header("References")]
    [SerializeField] private UIController ui;
    [SerializeField] private GameController game;

    private Vector2 fingerMotion;
    private Vector2 startingTouchPos;
    private Vector2 lastTouchPos = Vector2.zero;
    private Vector2 endTouchPos;
    private float touchTime = 0;

    private bool overUI = false;
    private bool dragginScreen = false;
    private bool zooming = false;
    private bool noInput = true;

    private bool lockMovement = false;
    private bool lockZoom = false;
    private bool lockTap = false;

    public static Action<Vector3> OnTap;
    public static Action<Vector2> OnCameraMoved;
    public static Action<float> OnCameraZoom;

    private TargetDevice targetDevice;

    private void Awake()
    {
        #if UNITY_IOS
           Debug.Log("Iphone");
           targetDevice = TargetDevice.IOS;
        #endif

        #if UNITY_ANDROID
            Debug.Log("Android");
            targetDevice = TargetDevice.ANDROID;
        #endif

        #if UNITY_WEBGL
            Debug.Log("Web gl");
            targetDevice = TargetDevice.WEB;
        #endif

        #if UNITY_EDITOR
           Debug.Log("Unity Editor");
           targetDevice = TargetDevice.WEB;
        #endif
    }

    private void Update()
    {
        switch (targetDevice)
        {
            case TargetDevice.ANDROID:
                HandleMobileInput();
                break;
            case TargetDevice.IOS:
                HandleMobileInput();
                break;
            case TargetDevice.WEB:
                HandleKeyboardAndMouseInput();
                break;
        }
    }

    private void HandleKeyboardAndMouseInput()
    {
        overUI = EventSystem.current.IsPointerOverGameObject();

        if (Input.GetMouseButtonDown(0) && !overUI)
        {
            touchTime = 0;
            startingTouchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0) && !overUI)
        {
            touchTime += Time.deltaTime;
            float dist = 0;
            if(!dragginScreen)
            {
                dist = Vector2.Distance(startingTouchPos, mainCam.ScreenToWorldPoint(Input.mousePosition));
            }
            //Debug.Log("Mouse Distace" + dist);
            if (!dragginScreen && dist > maxDistanceForMovedFlag)
            {
                dragginScreen = true;
            }
            if(dragginScreen)
            {
                if (!zooming && !lockMovement)
                {
                    Vector2 dir = startingTouchPos - (Vector2)mainCam.ScreenToWorldPoint(Input.mousePosition);
                    OnCameraMoved?.Invoke(dir);
                    vcam.transform.position += new Vector3(dir.x, dir.y, 0);
                }
            }
        }
        if (Input.GetMouseButtonUp(0) && !overUI)
        {
            endTouchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            vcam.transform.position = mainCam.transform.position;
            if (touchTime < maxTapTime && !dragginScreen && !zooming && !lockTap)
            {
                OnTap?.Invoke(endTouchPos);
            }
            dragginScreen = false;
        }
        if(!lockZoom)
        {
            ZoomCamera(Input.GetAxis("Mouse ScrollWheel") * wheelMouseZoomMult);
        }
    }

    private void HandleMobileInput()
    {
        if (Input.touchCount > 0) //detect touch over ui
        {
            foreach (Touch touch in Input.touches)
            {
                int id = touch.fingerId;
                if (EventSystem.current.IsPointerOverGameObject(id))
                {
                    overUI = true;
                }
            }
        }

        if (Input.touchCount == 2 && !dragginScreen && !overUI && !lockZoom) //Handle pinch zoom
        {
            noInput = false;
            zooming = true;
            Touch touchA = Input.GetTouch(0);
            Touch touchB = Input.GetTouch(1);
            Vector2 touchAPreviousPosition = touchA.position - touchA.deltaPosition;
            Vector2 touchBPreviousPosition = touchB.position - touchB.deltaPosition;
            float prevMag = (touchAPreviousPosition - touchBPreviousPosition).magnitude;
            float currentMag = (touchA.position - touchB.position).magnitude;
            float diff = currentMag - prevMag;
            ZoomCamera(diff * zoomMult);
        }
        if (Input.touchCount == 1 && !zooming && !overUI) //Tap and movement
        {
            noInput = false;
            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:
                    startingTouchPos = mainCam.ScreenToWorldPoint(t.position);
                    touchTime = 0;
                    break;
                case TouchPhase.Moved:
                    dragginScreen = true;
                    if (!zooming && !lockMovement)
                    {
                        fingerMotion = startingTouchPos - (Vector2)mainCam.ScreenToWorldPoint(t.position);
                        OnCameraMoved?.Invoke(fingerMotion);
                        vcam.transform.position += (new Vector3(fingerMotion.x, fingerMotion.y, 0) * camSpeed) * Time.deltaTime;
                    }
                    break;
                case TouchPhase.Stationary:
                    touchTime += Time.deltaTime;
                    break;
                case TouchPhase.Ended:
                    endTouchPos = mainCam.ScreenToWorldPoint(t.position);
                    vcam.transform.position = mainCam.transform.position;
                    if (touchTime < maxTapTime && !dragginScreen && !zooming && !lockTap)
                    {
                        OnTap?.Invoke(endTouchPos);
                    }
                    touchTime = 0;
                    endTouchPos = t.position;
                    break;
            }
        }
        if (Input.touchCount == 0) //clear all
        {
            overUI = false;
            noInput = true;
            zooming = false;
            dragginScreen = false;
        }
        //ClampCameraPosition();
    }

    public void LockInput(bool mov, bool zoom, bool tap)
    {
        lockMovement = mov;
        lockZoom = zoom;
        lockTap = tap;
    }

    public void MoveCameraToWorldPosition(Vector3 worldPos, float ortographicSize, float t, Action onMoveEnd)
    {
        Debug.Log("moving Camera");
        worldPos.z = vcam.transform.position.z;
        LeanTween.value(vcam.gameObject, (newVal) => { vcam.m_Lens.OrthographicSize = newVal; }, vcam.m_Lens.OrthographicSize, ortographicSize, t - 0.1f);
        LeanTween.move(vcam.gameObject, worldPos, t).setOnComplete(() => {
            Debug.Log("Camera moved!");
            onMoveEnd?.Invoke();
        });
    }

    private void ZoomCamera(float incrementValue)
    {
        vcam.m_Lens.OrthographicSize = Mathf.Clamp(vcam.m_Lens.OrthographicSize - incrementValue, zoomMin, zoomMax);
        OnCameraZoom?.Invoke(vcam.m_Lens.OrthographicSize);
    }
}

public enum TargetDevice
{
    ANDROID,
    IOS,
    WEB
}
