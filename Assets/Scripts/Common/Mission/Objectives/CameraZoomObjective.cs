using System;
using UnityEngine;

public class CameraZoomObjective : Objective
{
    float inVal = 0;
    float outVal = 0;

    bool zoomedIn = false;
    bool zoomedOut = false;

    public CameraZoomObjective(string _uiEntry, float zoomInVal, float zoomOutVal ,Action _OnComplete)
    {
        uiEntry = _uiEntry;
        inVal = zoomInVal;
        outVal = zoomOutVal;
        id = "Objective-ZoomCam-" + Utils.GenerateRandomString(5);
        completed = false;
        InputController.OnCameraZoom += FlagZoom;
        onComplete = _OnComplete;
    }

    private void FlagZoom(float amount)
    {
        if(amount < inVal)
        {
            zoomedIn = true;
        }
        if(amount > outVal)
        {
            zoomedOut = true;
        }

        if (zoomedIn && zoomedOut)
        {
            OnComplete?.Invoke();
            completed = true;
            InputController.OnCameraZoom -= FlagZoom;
            GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(3);
            stage.CheckIfStageIsCompleted();
        }
    }
}