using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using System;



public class Tutorial : MonoBehaviour
{
    [Header("UI things")]
    [SerializeField] private GameObject cover;
    [SerializeField] private GameObject money;
    [Header("References")]
    [SerializeField] private GameController gc;
    [SerializeField] private UIController ui;
    [SerializeField] private MissionController mc;
    [SerializeField] private InputController ic;
    [SerializeField] private CharController cc;


    private void Awake()
    {
        GameController.OnMainScriptReady += SetupTutorial;
    }

    private void SetupTutorial()
    {
        GameController.OnMainScriptReady -= SetupTutorial;
        gc.SwitchState(1);
        cc.AssignAnActionAtEndOfDialog(0, () =>
        {
            mc.CreateCameraMission();
            gc.SwitchState(0);
            ic.LockInput(false, true, true);
        });
        cc.AssignAnActionAtEndOfDialog(1, () =>
        {
            mc.CreateZoomMission();
            ic.LockInput(true, false, true);
            gc.SwitchState(0);
        });
        cc.AssignAnActionAtEndOfDialog(2, () =>
        {
            Vector3 tgtPos = gc.CellToWorldPosition(new Vector3Int(7, 5, 0)) + new Vector3(0, 0.5f, 0);
            ic.MoveCameraToWorldPosition(tgtPos, 2, 1,() =>
            {
                gc.SwitchState(1);
                ui.AddAttentionBubble("Tutorial bubble", new Vector3Int(7, 5, 0));
                ui.WaitAndExecuteFunction(1, () => {
                    ui.GetComponent<CharController>().ShowDialog(3);
                });
            });
        });
        cc.AssignAnActionAtEndOfDialog(3, () => //tile selection
        {
            gc.SwitchState(0);
            ic.LockInput(true, true, false);
            gc.LimitSelectionOfTiles(new Vector3Int(7, 5, 0), () =>
            {
                ic.LockInput(true, true, true);
                gc.UnlimitSelectionsOfTiles();
                cc.ShowDialog(4);
                Debug.Log("Context menu opened! Limit removed and bubble Locked");
            });
        });
        cc.AssignAnActionAtEndOfDialog(4, () => //pop up
        {
            ui.ButtonPressed = (s) => {
                if (s == "Components")
                {
                    cc.ShowDialog(5);
                    ui.FadeInMenu(money);
                    Debug.LogWarning("Component bubble button pressed");
                    ui.ButtonPressed = null;
                }
            };
        });
        cc.AssignAnActionAtEndOfDialog(5, () => //Categorias
        {
            string[] buttonsToDisable = new string[] {
                "But_CatExit",
                "CUBIERTAS",
                "INTERRUPTORES",
                "TOMACORRIENTES",
                "SENSORES",
                "TERMICAS",
                "But_CompExit"
            };
            ui.DisableButton(buttonsToDisable);
            ui.ButtonPressed = (s) => {
                if (s == "ILUMINACION")
                {
                    cc.ShowDialog(6);
                    Debug.LogWarning("Boton Iluminacion presionado");
                    ui.ButtonPressed = null;
                }
            };
        });
        cc.AssignAnActionAtEndOfDialog(6, () => //Explain Bar
        {
            ui.ButtonPressed = (s) => {
                if (s == "0" || s == "1" || s == "2" || s == "3")
                {
                    string[] buttonsToDisable = new string[] {
                        "0",
                        "1",
                        "2",
                        "3"
                    };
                    ui.DisableButton(buttonsToDisable);
                    cc.ShowDialog(7);
                    Debug.LogWarning("Producto Comprado");
                    ui.EnableADisabledButton("But_CompExit");
                    ui.EnableADisabledButton("But_CatExit");
                    ui.ButtonPressed = (exit) => {
                        if (exit == "But_CatExit")
                        {
                            ic.MoveCameraToWorldPosition(gc.CellToWorldPosition(new Vector3Int(8, 8, 0)), 3, 1,() => {
                                cc.ShowDialog(8);
                                ui.ButtonPressed = null;
                            });
                        }
                    };
                }
            };
        });
        cc.AssignAnActionAtEndOfDialog(8, () =>
        {
            gc.LimitSelectionOfTiles(new Vector3Int(8, 7, 0), () => {
                cc.ShowDialog(10);
            });
            ic.MoveCameraToWorldPosition(gc.CellToWorldPosition(new Vector3Int(8, 7, 0)) + new Vector3(0, 0.5f, 0), 0.8f, 1 ,() => {
                cc.ShowDialog(9);
                ui.ButtonPressed = null;
            });
        });
        cc.AssignAnActionAtEndOfDialog(9, () =>
        {
            ic.LockInput(true, true, false);
        });
        cc.AssignAnActionAtEndOfDialog(10, () =>
        {
            ic.LockInput(true, true, true);
            ui.ButtonPressed = (exit) => {
                if (exit == "Buy")
                {
                    cc.ShowDialog(11);
                    ui.ButtonPressed = null;
                    gc.UnlimitSelectionsOfTiles();
                    gc.LimitSelectionOfTiles(new Vector3Int(8, 7, 0), null);
                    string[] buttonsToEnable = new string[] {
                        "But_CatExit",
                        "CUBIERTAS",
                        "INTERRUPTORES",
                        "TOMACORRIENTES",
                        "SENSORES",
                        "TERMICAS",
                        "But_CompExit"
                    };
                    ui.EnableButtons(buttonsToEnable);
                    Debug.LogWarning("Error Step");
                    ui.EnableAllButtons();
                    ui.DisableButton(new string[] { "But_BuildExit" });
                }
            };

        });
        cc.AssignAnActionAtEndOfDialog(11, () =>
        {
            ic.LockInput(true, true, false);
            ui.ButtonPressed = (build) => {
                if (build == "Build")
                {
                    cc.ShowDialog(12);
                    ui.ButtonPressed = null;
                }
            };
        });
        cc.AssignAnActionAtEndOfDialog(12, () =>
        {
            mc.CreateAnyBuildingMission();
        });
        cc.AssignAnActionAtEndOfDialog(13, () =>
        {
            ic.MoveCameraToWorldPosition(gc.CellToWorldPosition(new Vector3Int(7,5,0)), 3, 1, () =>
            {
                cc.ShowDialog(14);
            });
        });
        cc.AssignAnActionAtEndOfDialog(14, () =>
        {
            ic.LockInput(true, true, true);
            gc.SpawnMoneyBubbles();
            ui.ButtonPressed = (build) => {
                if (build == "87 - money")
                {
                    cc.ShowDialog(15);
                    ui.ButtonPressed = null;
                }
            };
            
        });
        cc.AssignAnActionAtEndOfDialog(15, () =>
        {
            gc.SwitchState(0);
            ic.LockInput(false, false, false);
            gc.UnlimitSelectionsOfTiles();
            ui.EnableAllButtons();
        });

        //Start Tutorial
        gc.SetMoney(10000);
        cover.SetActive(true);
        ui.SetAlpha(cover, 1, 0, 2);
        ic.MoveCameraToWorldPosition(new Vector3(0, 4, -30), 3, 3,() =>
          {
              ui.SetDirectionOfFade(2);
              cc.ShowDialog(0);
          });
    }
}
