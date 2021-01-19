using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private float camSpeed = 10f;
    [SerializeField] private float maxTapTime = 0.2f;
    [Header("Zoom")]
    [SerializeField] private float zoomMult = 0.01f;
    [SerializeField] private float zoomMin = 3f;
    [SerializeField] private float zoomMax = 8f;

    private Vector2 fingerMotion;
    private Vector2 startingTouchPos;
    private Vector2 lastTouchPos = Vector2.zero;
    private Vector2 endTouchPos;
    private float touchTime = 0;
    private bool dragginScreen = false;
    private bool pinch = false;

    public static Action<Vector3> OnTap;

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        
        if (Input.touchCount == 1 && !pinch)
        {
            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:
                    dragginScreen = false;
                    startingTouchPos = mainCam.ScreenToWorldPoint(t.position);
                    touchTime = 0;
                    break;
                case TouchPhase.Moved:
                    dragginScreen = true;
                    if(!pinch)
                    {
                        fingerMotion = startingTouchPos - (Vector2)mainCam.ScreenToWorldPoint(t.position);
                        mainCam.transform.position += (new Vector3(fingerMotion.x, fingerMotion.y, 0) * camSpeed) * Time.deltaTime;
                    }
                    break;
                case TouchPhase.Stationary:
                    touchTime += Time.deltaTime;
                    break;
                case TouchPhase.Ended:
                    endTouchPos = mainCam.ScreenToWorldPoint(t.position);
                    if (touchTime < maxTapTime && !dragginScreen && !pinch)
                    {
                        OnTap?.Invoke(endTouchPos);
                    }
                    touchTime = 0;
                    endTouchPos = t.position;
                    break;
            }
        }
        else if(Input.touchCount == 2)
        {
            pinch = true;
            Touch touchA = Input.GetTouch(0);
            Touch touchB = Input.GetTouch(1);
            Vector2 touchAPreviousPosition = touchA.position - touchA.deltaPosition;
            Vector2 touchBPreviousPosition = touchB.position - touchB.deltaPosition;
            float prevMag = (touchAPreviousPosition - touchBPreviousPosition).magnitude;
            float currentMag = (touchA.position - touchB.position).magnitude;
            float diff = currentMag - prevMag;

            ZoomCamera(diff*zoomMult);
        }
        else if(Input.touchCount == 0)
        {
            if(pinch)
            {
                pinch = false;
            }
        }
        MouseMovement();
        ZoomCamera(Input.GetAxis("Mouse ScrollWheel"));
    }

    private void MouseMovement()
    {
        if(Input.GetMouseButtonDown(0))
        {
            startingTouchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        }
        else if(Input.GetMouseButton(0))
        {
            fingerMotion = startingTouchPos - (Vector2)mainCam.ScreenToWorldPoint(Input.mousePosition);
            mainCam.transform.position += (new Vector3(fingerMotion.x, fingerMotion.y, 0) * camSpeed) * Time.deltaTime;
        }
    }

    private void ZoomCamera(float incrementValue)
    {
        mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize - incrementValue, zoomMin, zoomMax);
    }
}
