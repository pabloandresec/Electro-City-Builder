using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cutscene : MonoBehaviour
{
    public void LoadLevel(int index)
    {
        Scene selectedScene = SceneManager.GetSceneByBuildIndex(index);
        if(selectedScene != null)
        {
            Utils.LoadLevelAsync(selectedScene.buildIndex);
        }
    }
}
