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
    [Header("Clamp")]
    [SerializeField] private FloatRange yClampRange = new FloatRange(0, 8);
    [SerializeField] private FloatRange xClampRange = new FloatRange(-8, 8);

    private Vector2 fingerMotion;
    private Vector2 startingTouchPos;
    private Vector2 lastTouchPos = Vector2.zero;
    private Vector2 endTouchPos;
    private float touchTime = 0;

    private bool overUI = false;
    private bool dragginScreen = false;
    private bool zooming = false;
    private bool noInput = true;

    public static Action<Vector3> OnTap;

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

        if (Input.touchCount == 2 && !dragginScreen && !overUI)
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
                    if (!zooming)
                    {
                        fingerMotion = startingTouchPos - (Vector2)mainCam.ScreenToWorldPoint(t.position);
                        vcam.transform.position += (new Vector3(fingerMotion.x, fingerMotion.y, 0) * camSpeed) * Time.deltaTime;
                    }
                    break;
                case TouchPhase.Stationary:
                    touchTime += Time.deltaTime;
                    break;
                case TouchPhase.Ended:
                    endTouchPos = mainCam.ScreenToWorldPoint(t.position);
                    vcam.transform.position = mainCam.transform.position;
                    if (touchTime < maxTapTime && !dragginScreen && !zooming)
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

    private void ClampCameraPosition()
    {
        float xPos = Mathf.Clamp(mainCam.transform.position.x, xClampRange.min, xClampRange.max);
        float yPos = Mathf.Clamp(mainCam.transform.position.y, yClampRange.min, yClampRange.max);
        float zPos = vcam.transform.position.z;
        mainCam.transform.position = new Vector3(xPos, yPos, zPos);
    }

    private void MouseMovement()
    {
        noInput = false;
        
        if (Input.GetMouseButtonDown(2))
        {
            startingTouchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        }
        else if(Input.GetMouseButton(2))
        {
            dragginScreen = true;
            fingerMotion = startingTouchPos - (Vector2)mainCam.ScreenToWorldPoint(Input.mousePosition);
            mainCam.transform.position += (new Vector3(fingerMotion.x, fingerMotion.y, 0) * camSpeed) * Time.deltaTime;
        }
    }

    private void ZoomCamera(float incrementValue)
    {
        vcam.m_Lens.OrthographicSize = Mathf.Clamp(vcam.m_Lens.OrthographicSize - incrementValue, zoomMin, zoomMax);
    }
}
