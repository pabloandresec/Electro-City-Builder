using System;
using UnityEngine;

public class CameraMovementObjective : Objective
{
    bool left  = false;
    bool right = false;
    bool up    = false;
    bool down  = false;

    public CameraMovementObjective(string _uiEntry, Action _OnComplete)
    {
        uiEntry = _uiEntry;
        id = "Objective-MoveCam-" + Utils.GenerateRandomString(5);
        completed = false;
        InputController.OnCameraMoved += FlagDirections;
        onComplete = _OnComplete;
    }

    private void FlagDirections(Vector2 direction)
    {
        float angle = Vector2.SignedAngle(new Vector2(1, 1).normalized, direction);
        //Debug.Log("Drag angle = " + angle);

        if(angle < 0 && angle > -90)
        {
            right = true;
        }
        if (angle < -90 && angle > -179)
        {
            down = true;
        }
        if (angle < 179 && angle > 90)
        {
            left = true;
        }
        if (angle < 90 && angle > 0)
        {
            up = true;
        }

        if (left && right && up && down)
        {
            OnComplete?.Invoke();
            completed = true;
            InputController.OnCameraMoved -= FlagDirections;
            GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(3);
            stage.CheckIfStageIsCompleted();
        }
    }
}