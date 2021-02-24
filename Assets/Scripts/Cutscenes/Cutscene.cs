using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class Cutscene : MonoBehaviour {
    private PlayableDirector director;

    private void Awake() {
        director = GetComponent<PlayableDirector>();
    }


    private void Update() {
        if (director.state == PlayState.Paused && Input.anyKeyDown) {
            director.Play();
        }
    }

    public void WaitForInput() {
        director.Pause();
    }

    public void LoadLevel(int index)
    {
        Scene selectedScene = SceneManager.GetSceneByBuildIndex(index);
        if(selectedScene != null)
        {
            Utils.LoadLevelAsync(selectedScene.buildIndex);
        }
    }
}
