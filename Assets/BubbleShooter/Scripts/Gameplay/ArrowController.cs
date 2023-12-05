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

public class ArrowController : MonoBehaviour
{

    int lengthNormal = 1;
    public GameObject DotObject;

    public List<GameObject> ArrowParts = new List<GameObject>();
    public GameplayController gameplayController;

    public GameObject startPoint;

    public int currentID = -1;
    public int LimitLevel = 0; // Lowest Level to show full arrow
    void Awake()
    {
        transform.position = startPoint.transform.position;
    }


    bool stopped = false;
    float bounceY = 0;//, bounceX = 0;
    public void UpdateArrow()
    {
        stopped = false;
        bounceY = 0; //bounceX = 0;

        for (int i = 0; i < ArrowParts.Count; i++)
        {
            GameObject arrowpart = ArrowParts[i];

            if (stopped)
            {
                arrowpart.SetActive(false);
                continue;
            }

            arrowpart.SetActive(CheckPos(arrowpart.transform.position));
        }

    }

    public void UpdateDotColor(int id)
    {
        if (id < 1 || id > 10) return;

        if (id == currentID) return;

        currentID = id;
        
        for (int i = 0; i < ArrowParts.Count; i++)
        {
            GameObject part = ArrowParts[i];
            SpriteRenderer srenderer = part.GetComponent<SpriteRenderer>();
        }
    }

    bool CheckPos(Vector3 pos)
    {
        bool result = true;
        if (gameplayController._bubbleBoard.MeetAnyBubble(pos))
        {
            result = false;
            stopped = true;
        }
        else if (pos.x < GameplayController.LeftWall + Bubble.BUBBLE_RADIUS / 2)
        {
            result = false;
            stopped = true;
            bounceY = pos.y;
        }
        else if (pos.x > GameplayController.RightWall - Bubble.BUBBLE_RADIUS / 2)
        {
            result = false;
            stopped = true;
            bounceY = pos.y;
        }
        return result;
    }
}
