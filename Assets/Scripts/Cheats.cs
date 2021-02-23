using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cheats : MonoBehaviour
{
    [SerializeField] private GameController game;
    int count = 0;

    public void CheatMoney()
    {
        count++;
        if(count == 10)
        {
            game.AddMoney(50000);
            count = 0;
        }
    }
}
