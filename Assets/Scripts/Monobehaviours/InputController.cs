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
    [Header("Zoom")]
    [SerializeField] private float zoomMult = 0.01f;
    [SerializeField] private float zoomMin = 3f;
    [SerializeField] private float zoomMax = 8f;
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

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.touchCount > 0)
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

        if (Input.touchCount == 2 && !dragginScreen && !overUI && !lockZoom)
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
        if (Input.touchCount == 1 && !zooming && !overUI)
        {
            noInput = false;
            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:
                    startingTouchPos = mainCam.ScreenToWorldPoint(t.position);
                    //cam.transform.position = mainCam.transform.position;
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
        if (Input.touchCount == 0)
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
