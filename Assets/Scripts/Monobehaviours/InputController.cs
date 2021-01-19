using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private float camSpeed = 10f;
    [SerializeField] private float maxTapTime = 0.2f;

    private Vector2 fingerMotion;
    private Vector2 startingTouchPos;
    private Vector2 lastTouchPos = Vector2.zero;
    private Vector2 endTouchPos;
    private float touchTime = 0;

    public static Action<Vector3> OnTap;

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:
                    startingTouchPos = mainCam.ScreenToWorldPoint(t.position);
                    touchTime = 0;
                    break;
                case TouchPhase.Moved:
                    fingerMotion = startingTouchPos - (Vector2)mainCam.ScreenToWorldPoint(t.position);
                    mainCam.transform.position += (new Vector3(fingerMotion.x, fingerMotion.y, 0) * camSpeed) * Time.deltaTime;
                    break;
                case TouchPhase.Stationary:
                    touchTime += Time.deltaTime;
                    break;
                case TouchPhase.Ended:
                    endTouchPos = mainCam.ScreenToWorldPoint(t.position);
                    if (touchTime < maxTapTime)
                    {
                        OnTap?.Invoke(endTouchPos);
                    }
                    touchTime = 0;
                    endTouchPos = t.position;
                    break;
            }
        }
    }
}
