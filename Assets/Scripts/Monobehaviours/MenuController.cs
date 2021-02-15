using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject[] mainMenus;
    [Range(0,4)]
    [SerializeField] private int fadeDirection = 0;
    [SerializeField] private float fadeTime = 0.2f;
    private bool swapingMenus;
    private Dictionary<string, Vector2> menuPositions;

    private void Start()
    {
        StoreMenuPositions();
        SetupButtonFunctionality();
    }


    /// <summary>
    /// Asigna las funciones de los botones
    /// </summary>
    private void SetupButtonFunctionality()
    {
        //Tutorial button
        Transform butTutorial = transform.Find("Men_Main/Spc_Buttons/But_Tutorial");
        if(butTutorial != null)
        {
            butTutorial.GetComponent<Button>().onClick.AddListener(() =>
            {
                FadeSwapMenu(mainMenus[0], mainMenus[1], () => { Utils.LoadLevelAsync(1); });
            });
        }
        //Exit button
        Transform butExitApp = transform.Find("Men_Main/Spc_Buttons/But_Salir");
        if(butExitApp != null)
        {
            butExitApp.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Quitting!");
                Application.Quit();
            });
        }
        
    }

    /// <summary>
    /// Almacena todos los grupos de menus en un diccionario para su posterior uso en fades
    /// </summary>
    private void StoreMenuPositions()
    {
        CanvasGroup[] canvases = Resources.FindObjectsOfTypeAll<CanvasGroup>();
        menuPositions = new Dictionary<string, Vector2>();
        for (int i = 0; i < canvases.Length; i++)
        {
            menuPositions.Add(canvases[i].transform.name, canvases[i].GetComponent<RectTransform>().anchoredPosition);
        }
        Debug.Log(canvases.Length + " menu positions saved for later use");
    }

    /// <summary>
    /// Cambia inmediatamente de menu
    /// </summary>
    /// <param name="i"></param>
    public void SwitchMenu(int i)
    {
        if (i < 0 || i >= mainMenus.Length)
        {
            Debug.LogError("Out of index error in menus array");
        }
        foreach (GameObject m in mainMenus)
        {
            m.SetActive(false);
        }
        mainMenus[i].SetActive(true);
    }

    /// <summary>
    /// Cambia el alpha de algun grupo
    /// </summary>
    /// <param name="menuGroup"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    private void SetAlpha(CanvasGroup menuGroup, float from, float to)
    {
        menuGroup.interactable = false;
        menuGroup.alpha = from;
        LeanTween.value(menuGroup.gameObject, (v) => { menuGroup.alpha = v; }, from, to, fadeTime).setOnComplete(() =>
        {
            if (menuGroup.alpha == 1)
            {
                menuGroup.interactable = true;
            }
        });
    }

    /// <summary>
    /// Setea la direccion en la que los menus iran mientras estan en fade
    /// </summary>
    /// <param name="direction"></param>
    public void SetFadeDirection(int direction)
    {
        fadeDirection = Mathf.Clamp(direction,0,5);
        Debug.Log("Direction");
    }

    /// <summary>
    /// De Acuredo a la direccion de "directionToFadeFrom" calcular el destino del fade
    /// </summary>
    /// <param name="rt"></param>
    /// <returns></returns>
    private Vector2 GetFadeTargetPosition(RectTransform rt)
    {
        Vector2 t = Vector2.zero;
        switch (fadeDirection)
        {
            case 0: //North
                t += new Vector2(0, rt.rect.size.y / 2);
                break;
            case 1: //East
                t += new Vector2(rt.rect.size.x / 2, 0);
                break;
            case 2: //South
                t -= new Vector2(0, rt.rect.size.y / 2);
                break;
            case 3: //West
                t -= new Vector2(rt.rect.size.x / 2, 0);
                break;
        }

        return t;
    }

    #region FadeMenu
    public void FadeInMenu(GameObject menu)
    {
        swapingMenus = true;

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        Vector2 intialPos = menuRect.anchoredPosition + GetFadeTargetPosition(menuRect);
        menuRect.anchoredPosition = intialPos;
        //Debug.Log("getting " + menu.transform.name + " og pos");
        Vector2 tgtPos = menuPositions[menu.transform.name];

        SetAlpha(menu.GetComponent<CanvasGroup>(), 0, 1);
        menu.SetActive(true);
        LeanTween.value(menu, (vec) =>
        {
            menuRect.anchoredPosition = vec;
        }, intialPos, tgtPos, fadeTime).setOnComplete(() =>
        {
            swapingMenus = false;
        });
    }

    public void FadeOutMenu(GameObject menu)
    {
        swapingMenus = true;

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        Vector2 intialPos = menuRect.anchoredPosition;
        Vector2 tgtPos = menuRect.anchoredPosition + GetFadeTargetPosition(menuRect);

        SetAlpha(menu.GetComponent<CanvasGroup>(), 1, 0);
        LeanTween.value(menu, (vec) =>
        {
            menuRect.anchoredPosition = vec;
        }, intialPos, tgtPos, fadeTime).setOnComplete(() =>
        {
            swapingMenus = false;
            menu.SetActive(false);
        });
    }

    public void FadeOutMenu(GameObject menu, Action _OnFadeEnd)
    {
        swapingMenus = true;

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        Vector2 intialPos = menuRect.anchoredPosition;
        Vector2 tgtPos = menuRect.anchoredPosition + GetFadeTargetPosition(menuRect);

        SetAlpha(menu.GetComponent<CanvasGroup>(), 1, 0);
        LeanTween.value(menu, (vec) =>
        {
            menuRect.anchoredPosition = vec;
        }, intialPos, tgtPos, fadeTime).setOnComplete(() =>
        {
            swapingMenus = false;
            menu.SetActive(false);
            _OnFadeEnd?.Invoke();
        });
    }

    public void FadeSwapMenu(GameObject current, GameObject next, Action _OnFadeEnd)
    {
        fadeDirection = 1;
        FadeInMenu(next);
        FadeOutMenu(current, _OnFadeEnd);
    }
    #endregion
}
