/*
 ##########################################################
 ##########################################################
 -----------------   created by David ---------------------
 -----------------   created date : 12/03/2023 ------------
 ##########################################################
 ##########################################################
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobalData
{

    public static int WHITE_COUNT = 6;

    public const float DOWN_TIME = 8f;

    // GLOBAL 
    public static bool REDUCED_VERSION = false;

    // Fixed Data
    public static Dictionary<int, int[]> PreBubble = new Dictionary<int, int[]>();
    public static int[,] PointReward = new int[15, 3];
    public static bool DataLoaded = false;
    // Temporal Data

    static int Level = 1;

    public static int GetCurrentLevel()
    {
        return Level;
    }
    public static void SetCurrentLevel(int lvl)
    {
        Level = lvl;
    }

    public static void LoadLevelData()
    {
        // PreLoad Allthings
        if (DataLoaded) return;
        // Level pass require

        DataLoaded = true;
    }
}
