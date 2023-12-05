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

public class IBubbleBoard : MonoBehaviour {

    virtual public bool isDoneLoading() { return false; }
    virtual public MapData getMapData() { return null; }
    virtual public List<Bubble> getBubbleList() { return null; }

    virtual public void InitBubbleBoard() { }


    virtual public void AddFlyingBubble(Bubble b) { }

    virtual public int GetRandomColor() { return 0; }

    virtual public void DropEveryThing() { }

    virtual public bool MeetAnyBubble(Vector3 pos) { return false; }

}
