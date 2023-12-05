/*
 ##########################################################
 ##########################################################
 -----------------   created by David ---------------------
 -----------------   created date : 12/03/2023 ------------
 ##########################################################
 ##########################################################
*/

using UnityEngine;
using System.Collections.Generic;

public class MapData
{
    public bool isStartWithLeft; // Check if the last row is left 
    public int BubbleNumber; // Number of Bubble to shoot
    public int MapSizeY;
    public List<int[]> BubbleData; // x, y, id 

    public void LoadMapData()
    {
        isStartWithLeft = false;
        MapSizeY = 0;
    }

}