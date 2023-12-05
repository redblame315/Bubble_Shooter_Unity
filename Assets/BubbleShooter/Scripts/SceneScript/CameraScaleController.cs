/*
 ##########################################################
 ##########################################################
 -----------------   created by David ---------------------
 -----------------   created date : 12/04/2023 ------------
 ##########################################################
 ##########################################################
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScaleController : MonoBehaviour
{
    void Awake()
    {
        CameraScale();
    }

    void CameraScale()
    {
        if (Screen.width < Screen.height)
        {
            GetComponent<Camera>().orthographicSize = Bubble.BUBBLE_RADIUS * 9.5f / Screen.width * Screen.height;
        }
        else
        {
            GetComponent<Camera>().orthographicSize = Bubble.BUBBLE_RADIUS * 16f / 2;
        }
    }
}
