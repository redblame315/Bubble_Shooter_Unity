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

public class BubbleBoard : IBubbleBoard
{
    public bool needUpdateBoard = true, needUpdateMoving = true;
    // base bubble
    public GameObject BubblePrefab;
    public GameplayController _gameplayController;
    // Map
    public MapData mapData;
    int _currentRowCount = 0;
    int _bubbleEachRow = 17;
    public int bubbleTypes = 5;

    const float BottomLimit = -1.0f;

    // Loading
    bool didFinishLoading = false;
    int loadingRowsPerFrame = 1;
    int initBubbleRow = 9;
    int currentLoadState = 0;

    // bubble management

    public List<Bubble> _bubbleList;
    public float lowestBubblePos, highestBubblePos;
    public int lowestBubbleY;


    public List<Bubble> _nextProcessList, _connectedList;
    Bubble _lastFlyingBubble;

    // const
    float reduceY = 0f;
    public float movingspeed = 0.03f;

    // properties
    const float DANGER_TIME = 10f, PULL_DOWN_TIME = 0.2f;

    public float pullDownTime = 0;
    public float moveBackTime = 0;

    public bool onWarning = false;
    public float dangerTimer = DANGER_TIME;


    // getter
    public override bool isDoneLoading()
    {
        return didFinishLoading;
    }

    public override MapData getMapData()
    {
        return mapData;
    }

    public override List<Bubble> getBubbleList()
    {
        return _bubbleList;
    }


    // Use this for initialization
    void Start()
    {
        needUpdateBoard = true;
        needUpdateMoving = true;

        dangerTimer = DANGER_TIME;

        _gameplayController = GameObject.FindObjectOfType<GameplayController>();

    }

    public override void InitBubbleBoard()
    {
        didFinishLoading = false;

        mapData = new MapData();

        mapData.LoadMapData();

        this.transform.position = new Vector3(-Bubble.BUBBLE_RADIUS * 16.5f / 2 , Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 9f, this.transform.position.z);

        currentLoadState = 0;

        _currentRowCount = 0;

    }

    // Special spawn rate

    void CreateBubbleRow()
    {
        for (int i = 0; i < _bubbleEachRow; i++)
        {
            int color = Random.Range(0, bubbleTypes) + 1;
            if (didFinishLoading)
            {
                
                Debug.Log("here did finishe loading");

            }

            CreateBubbleAt(i, _currentRowCount, color);
        }
        _currentRowCount++;
    }

    Bubble CreateBubbleAt(int x, int y, int color)
    {
        GameObject bubble = Instantiate(BubblePrefab, transform.position, BubblePrefab.transform.rotation) as GameObject;
        bubble.transform.parent = transform;

        float fx = x * Bubble.BUBBLE_RADIUS + (CheckLeftDisplacement(y) ? 0 : 1) * Bubble.BUBBLE_RADIUS / 2;
        float fy = y * (Bubble.BUBBLE_RADIUS + reduceY);
        Vector3 pos = new Vector3(fx, fy, -0.5f);

        bubble.transform.localPosition = pos;

        Bubble bubbleControl = bubble.GetComponent("Bubble") as Bubble;
        bubbleControl.SetBubbleColor(color);

        bubbleControl._positionX = x;
        bubbleControl._positionY = y;

        _bubbleList.Add(bubbleControl);

        SetBubbleLinks(bubbleControl);

        return bubbleControl;
    }

    public override void AddFlyingBubble(Bubble b)
    {
        // Welcome, b*****
        _gameplayController._flyingBubbles.Remove(b);
        // Play sound

        b.isMoving = false;
        b.gameObject.tag = "bubble";

        // resetPosition
        b.transform.parent = this.transform;

        Vector3 velocity = b.velocity / 10;

        // get base y
        int y = (int)System.Math.Round((b.transform.localPosition.y) / (Bubble.BUBBLE_RADIUS + reduceY), System.MidpointRounding.ToEven);
        int x = (int)System.Math.Round((b.transform.localPosition.x - (CheckLeftDisplacement(y) ? 0 : Bubble.BUBBLE_RADIUS / 2)) / Bubble.BUBBLE_RADIUS, System.MidpointRounding.ToEven);

        while (GetBubbleAtGrid(x, y) != null)
        {
            b.transform.position -= velocity;

            if (b.transform.position.x <= GameplayController.LeftWall + Bubble.BUBBLE_RADIUS / 2)
            {
                velocity.x *= -1;
                b.transform.position = new Vector3(GameplayController.LeftWall + Bubble.BUBBLE_RADIUS / 2, b.transform.position.y, b.transform.position.z);
            }

            else if (b.transform.position.x >= GameplayController.RightWall - Bubble.BUBBLE_RADIUS / 2)
            {
                velocity.x *= -1;
                b.transform.position = new Vector3(GameplayController.RightWall - Bubble.BUBBLE_RADIUS / 2, b.transform.position.y, b.transform.position.z);
            }

            y = (int)System.Math.Round((b.transform.localPosition.y) / (Bubble.BUBBLE_RADIUS + reduceY), System.MidpointRounding.ToEven);
            x = (int)System.Math.Round((b.transform.localPosition.x - (CheckLeftDisplacement(y) ? 0 : Bubble.BUBBLE_RADIUS / 2)) / Bubble.BUBBLE_RADIUS, System.MidpointRounding.ToEven);
        }

        float fx = 0;
        float fy = 0;
        Vector3 okgrid;

        fx = x * Bubble.BUBBLE_RADIUS;
        if (!CheckLeftDisplacement(y)) fx += Bubble.BUBBLE_RADIUS / 2;
        fy = y * (Bubble.BUBBLE_RADIUS + reduceY);

        okgrid = new Vector3(fx, fy, -0.5f);

        b.transform.localPosition = okgrid;

        b._positionX = x;
        b._positionY = y;

        SetBubbleLinks(b);
        _bubbleList.Insert(0, b);



        List<Bubble> aroundList = new List<Bubble>();
        GetConnectedBubble(b, aroundList, 2, 1);
        

        aroundList.Clear();
        GetConnectedBubble(b, aroundList, 1, 1);
        foreach (Bubble bb in aroundList)
        {
            if (bb == null) continue;
            if (bb.ID == 14)
            {
                ActiveThunderEffect(bb);
            }
            else if (bb.ID == 17)
            {
                ActivePlasmaEffect(b.ID);
                _nextProcessList.Add(bb);
            }
        }

        _lastFlyingBubble = b;

        // call update if needed
        needUpdateBoard = true;
        //needUpdateMoving = true;
    }

    void SetBubbleLinks(Bubble bubble)
    {
        int x = bubble._positionX;
        int y = bubble._positionY;
        int rx, ry;
        Bubble temp = null;

        //LEFT
        temp = null;
        rx = x - 1; ry = y;
        temp = GetBubbleAtGrid(rx, ry);
        if (temp != null)
        {
            bubble.linkedBubbles.Add(temp);
            if (!temp.linkedBubbles.Contains(bubble)) temp.linkedBubbles.Add(bubble);
        }
        // TOP LEFT
        temp = null;
        rx = x - (1 - (CheckLeftDisplacement(y) ? 0 : 1)); ry = y + 1;
        temp = GetBubbleAtGrid(rx, ry);
        if (temp != null)
        {
            bubble.linkedBubbles.Add(temp);
            if (!temp.linkedBubbles.Contains(bubble)) temp.linkedBubbles.Add(bubble);
        }
        // TOP RIGHT
        temp = null;
        rx = x + (CheckLeftDisplacement(y) ? 0 : 1); ry = y + 1;
        temp = GetBubbleAtGrid(rx, ry);
        if (temp != null)
        {
            bubble.linkedBubbles.Add(temp);
            if (!temp.linkedBubbles.Contains(bubble)) temp.linkedBubbles.Add(bubble);
        }
        // RIGHT
        temp = null;
        rx = x + 1; ry = y;
        temp = GetBubbleAtGrid(rx, ry);
        if (temp != null)
        {
            bubble.linkedBubbles.Add(temp);
            if (!temp.linkedBubbles.Contains(bubble)) temp.linkedBubbles.Add(bubble);
        }
        // BOTTOM LEFT
        temp = null;
        rx = x - (1 - (CheckLeftDisplacement(y) ? 0 : 1)); ry = y - 1;
        temp = GetBubbleAtGrid(rx, ry);
        if (temp != null)
        {
            bubble.linkedBubbles.Add(temp);
            if (!temp.linkedBubbles.Contains(bubble)) temp.linkedBubbles.Add(bubble);
        }
        // BOTTOM LEFT
        temp = null;
        rx = x + (CheckLeftDisplacement(y) ? 0 : 1); ry = y - 1;
        temp = GetBubbleAtGrid(rx, ry);
        if (temp != null)
        {
            bubble.linkedBubbles.Add(temp);
            if (!temp.linkedBubbles.Contains(bubble)) temp.linkedBubbles.Add(bubble);
        }
    }

    bool CheckLeftDisplacement(int y)
    {
        return (y % 2 == 0) == mapData.isStartWithLeft;
    }

    float GetMapDisplacement()
    {
        float result = Bubble.BUBBLE_RADIUS / 2;

        return result;
    }

    void GetConnectedBubble(Bubble bubble, List<Bubble> container, int times = -1, int condition = 0, int counter = 0)
    {
        // Out of step
        if (times == 0) return;

        /* condition is the condition to add in container
         * 0 : Matching color, need "color" to compare
         * 1 : get all connected
        */

        foreach (Bubble b in bubble.linkedBubbles)
        {
            if (b == null) continue;

            if (condition == 0 && bubble.ID == b.ID && b.isFreezed == false)
            {
                if (!container.Contains(b))
                {
                    if (bubble.rootDelay != null)
                    {
                        b.dieDelayCount = Mathf.Abs(bubble.rootDelay._positionX - b._positionX) + Mathf.Abs(bubble.rootDelay._positionY - b._positionY);
                        b.rootDelay = bubble.rootDelay;
                    }
                    else
                    {
                        b.dieDelayCount = 0;
                        b.rootDelay = b;
                    }
                    container.Add(b);
                    GetConnectedBubble(b, container, times - 1, condition, counter + 1);
                }

            }
            else if (condition != 0)
            {
                if (!container.Contains(b))
                {
                    if (bubble.rootDelay != null)
                    {
                        b.dieDelayCount = Mathf.Abs(bubble.rootDelay._positionX - b._positionX) + Mathf.Abs(bubble.rootDelay._positionY - b._positionY);
                        b.rootDelay = bubble.rootDelay;
                    }
                    else
                    {
                        b.dieDelayCount = 0;
                        b.rootDelay = b;
                    }
                    container.Add(b);
                    GetConnectedBubble(b, container, times - 1, condition, counter + 1);
                }

            }
        }


    }

    Vector3 FindNearestAvailableGrid(Vector3 position)
    {
        Vector3 okgrid;

        // get base y
        int y = (int)System.Math.Round((position.y) / (Bubble.BUBBLE_RADIUS + reduceY), System.MidpointRounding.ToEven);
        int x = (int)System.Math.Round((position.x) / Bubble.BUBBLE_RADIUS, System.MidpointRounding.ToEven);

        float fx = x * Bubble.BUBBLE_RADIUS + (y % 2) * Bubble.BUBBLE_RADIUS / 2;
        float fy = y * (Bubble.BUBBLE_RADIUS + reduceY);

        okgrid = new Vector3(fx, fy, -0.5f);
        return okgrid;
    }

    public override int GetRandomColor()
    {
        int newId = -1;
        if (_bubbleList.Count > 0)
        {
            while (newId <= 0 || newId >= 11)
                newId = _bubbleList[Random.Range(0, _bubbleList.Count - 1)].ID;
        }

        return newId;
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameplayController.gameEnded)
        {
            return;
        }
        if (_gameplayController.gameStarted)
        {

            if (needUpdateBoard)
            {
                needUpdateBoard = false;
                needUpdateMoving = true;

                UpdateProcessList();
            }

            UpdateBoardedBubbles();
            _gameplayController.needUpdateArrowPosition = true;

            if (_gameplayController._flyingBubbles.Count > 0)
            {
                UpdateFlyingBubble();
            }
        }
        if (!didFinishLoading)
        {
            UpdateLoadingMap();
        }
    }

    void UpdateLoadingMap()
    {
        int cstate = currentLoadState;
        for (int i = 0; i < loadingRowsPerFrame; i++)
        {

            CreateBubbleRow();


            currentLoadState = cstate + i + 1;
            if (currentLoadState > initBubbleRow - 1)
            {
                didFinishLoading = true;
                break;
            }
        }
    }

    // Update Attached Bubble
    void UpdateBoardedBubbles()
    {
        lowestBubblePos = 4f;
        highestBubblePos = GameplayController.BotWall;
        lowestBubbleY = 0;
        foreach (Bubble b in _bubbleList)
        {
            if (b == null) continue;

            if (b.transform.position.y < lowestBubblePos)
            {
                lowestBubblePos = b.transform.position.y;
                lowestBubbleY = b._positionY;
            }


            if (b.transform.position.y > highestBubblePos)
                highestBubblePos = b.transform.position.y;

        }

        

        _gameplayController.LowestBubblePos = lowestBubblePos;

        if (Mathf.Abs(lowestBubblePos - (Camera.main.orthographicSize - 13f * Bubble.BUBBLE_RADIUS)) < 0.05f)
        {
            _gameplayController.gameEnded = true;
            GameSceneController.instance.ShowWinMenu();
            return;
        }

        // New bubble row
        if (highestBubblePos < (Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 1.5f))
        {
            CreateBubbleRow();
        }

    }

    // Update Flying Bubble And Catch 
    void UpdateFlyingBubble()
    {
        foreach (Bubble b in _gameplayController._flyingBubbles)
        {
            if (b == null) continue;
            if (b.transform.position.y < lowestBubblePos - Bubble.BUBBLE_RADIUS) continue;

            if (MeetAnyBubble(b.transform.position))
            {
                AddFlyingBubble(b);
                return;
            }
        }

    }

    void UpdateProcessList()
    {
        bool needUpdateDrop = false;
        // Update last Flying Bubble
        if (_lastFlyingBubble != null && _lastFlyingBubble.transform.position.y < (Camera.main.orthographicSize - 0.5f * Bubble.BUBBLE_RADIUS))
        {
            // Normal bubble
            if (_lastFlyingBubble.ID >= 1 && _lastFlyingBubble.ID <= 10)
            {
                _connectedList.Clear();
                _lastFlyingBubble.dieDelayCount = 0;
                _lastFlyingBubble.rootDelay = _lastFlyingBubble;
                _connectedList.Add(_lastFlyingBubble);
                GetConnectedBubble(_lastFlyingBubble, _connectedList);
                
                // Match 3, then explode
                if (_connectedList.Count >= 3)
                {
                    Debug.Log("explode");
                    _gameplayController.SetComboCount(_gameplayController.ComboCount > 14 ? _gameplayController.ComboCount : _gameplayController.ComboCount + 1);

                    _gameplayController.GiveComboReward();

                    foreach (Bubble b in _connectedList)
                    {
                        if (!_nextProcessList.Contains(b))
                            _nextProcessList.Add(b);
                    }

                }
                // No action
                else
                {
                    GlobalData.WHITE_COUNT--;
                    _gameplayController.DeleteWhiteBubble();
                    if (GlobalData.WHITE_COUNT == 0)
                    {
                        
                        //// Board updating function
                        float currentBoardY = transform.position.y;

                        currentBoardY -= Bubble.BUBBLE_RADIUS;

                        transform.position = new Vector3(transform.position.x, currentBoardY, transform.position.z);

                        GlobalData.WHITE_COUNT = 5;
                        _gameplayController.CreateInitWhiteBubbles();
                        //GameplayController.instance.AddWhiteBubble();
                    }
                    
                    _gameplayController.SetComboCount(0);
                }
                _connectedList.Clear();
                _lastFlyingBubble = null;
            }
            else if (_lastFlyingBubble.ID == 19) // rainbow
            {

                int newID = -1;
                foreach (Bubble b in _lastFlyingBubble.linkedBubbles)
                {
                    if (b.ID >= 1 && b.ID <= 10)
                    {
                        newID = b.ID;
                        _connectedList.Clear();
                        GetConnectedBubble(b, _connectedList, -1, 0);
                        if (_connectedList.Count >= 2)
                        {
                            _lastFlyingBubble.ID = b.ID;

                            _connectedList.Clear();
                            break;
                        }
                    }

                }

                if (_lastFlyingBubble.ID == 19) // No changes
                {
                    if (newID == -1)
                    {
                        newID = GetRandomColor();

                    }
                    _lastFlyingBubble.ID = newID;
                    _lastFlyingBubble.SetBubbleColor(newID, true);
                    _lastFlyingBubble = null;
                }

                needUpdateBoard = true;
            }
            else if (_lastFlyingBubble.ID == 20) // bomb
            {
                _connectedList.Clear();
                _lastFlyingBubble.dieDelayCount = 0;
                _lastFlyingBubble.rootDelay = _lastFlyingBubble;
                _connectedList.Add(_lastFlyingBubble);
                GetConnectedBubble(_lastFlyingBubble, _connectedList, 2, 1);

                _gameplayController.SetComboCount(_gameplayController.ComboCount + 1);

                _gameplayController.GiveComboReward();
                _nextProcessList.AddRange(_connectedList);

                _connectedList.Clear();
                _lastFlyingBubble = null;
            }
        }

        // Update queued process list
        List<Bubble> processCopy = new List<Bubble>();
        foreach (Bubble b in _nextProcessList)
        {
            processCopy.Add(b);
        }
        _nextProcessList.Clear();

        foreach (Bubble b in processCopy)
        {
            if (b == null) continue;
            if (b.KillID == 0)
            {
                // Kill it
                if (b.isFreezed == false)
                    _bubbleList.Remove(b);


                _nextProcessList.Remove(b);
                b.DisconnectBubble(true);
                _gameplayController.AddBubbleToKillList(b);
            }
        }

        if (processCopy.Count > 0)
        {
            needUpdateDrop = true;

            needUpdateBoard = true;

        }

        processCopy.Clear();

        if (needUpdateDrop)
        {
            // Dropping 
            //Collect all Top Row
            List<Bubble> topList = new List<Bubble>();
            foreach (Bubble b in _bubbleList)
            {
                if (b._positionY == _currentRowCount - 1 || b.ID == 15)
                    if (b != null && b.ID != 13)
                    {
                        topList.Add(b);
                    }
            }

            List<Bubble> securedList = new List<Bubble>();
            foreach (Bubble b in topList)
            {
                securedList.Add(b);
                GetConnectedBubble(b, securedList, -1, 1);
            }

            List<Bubble> dropList = new List<Bubble>();
            foreach (Bubble b in _bubbleList)
            {
                if (!securedList.Contains(b)) dropList.Add(b);
            }

            // Drop
            if (dropList.Count > 0)
            {
                _gameplayController.GiveComboReward(true, dropList.Count);

                foreach (Bubble b in dropList)
                {
                    _bubbleList.Remove(b);
                    
                    b.DisconnectBubble();
                    _gameplayController.AddBubbleToDropList(b);
                }

            }

            // Changing bullet
            _gameplayController.needUpdateBullet = true;
        }
    }


    void ActiveThunderEffect(Bubble root)
    {
        foreach (Bubble b in _bubbleList)
        {
            if (b._positionY == root._positionY)
            {
                b.dieDelayCount = Mathf.Abs(b._positionX - root._positionX);
                if (!_nextProcessList.Contains(b)) _nextProcessList.Add(b);
            }
        }
    }

    void ActivePlasmaEffect(int color)
    {
        int count = 1;
        foreach (Bubble b in _bubbleList)
        {
            if (b.ID == color)
            {
                b.dieDelayCount = count;
                count++;
                if (!_nextProcessList.Contains(b)) _nextProcessList.Add(b);
            }
        }
    }

    public override void DropEveryThing()
    {
        List<Bubble> dropList = new List<Bubble>();
        foreach (Bubble b in _bubbleList)
        {
            dropList.Add(b);
        }

        // Drop
        if (dropList.Count > 0)
        {
            foreach (Bubble b in dropList)
            {
                _bubbleList.Remove(b);
                b.DisconnectBubble();
                _gameplayController.AddBubbleToDropList(b);
            }
        }
    }

    Bubble GetBubbleAtGrid(int x, int y)
    {
        foreach (Bubble b in _bubbleList)
        {
            if (b == null) continue;
            if (b._positionX == x && b._positionY == y)
                return b;
        }
        return null;
    }

    

    public override bool MeetAnyBubble(Vector3 pos)
    {
        foreach (Bubble b in _bubbleList)
        {
            if (b == null) continue;
            if (b.linkedBubbles.Count >= 6) continue;
            if (Mathf.Abs(b.transform.position.y - pos.y) < (Bubble.BUBBLE_COLLIDE_RADIUS + reduceY) && Mathf.Abs(b.transform.position.x - pos.x) < Bubble.BUBBLE_COLLIDE_RADIUS)
                return true;
        }

        return false;
    }

}
