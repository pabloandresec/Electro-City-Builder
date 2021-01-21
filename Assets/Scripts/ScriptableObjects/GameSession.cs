using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "SessionData", menuName = "City Builder Data/Session Data")]
public class GameSession : ScriptableObject
{
    public string seed;
    public int money;
    public List<Building> activeBuildings;
}