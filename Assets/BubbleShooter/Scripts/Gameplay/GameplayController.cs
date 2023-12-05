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
using UnityEngine.UI;

public class GameplayController : MonoBehaviour
{
    public static GameplayController instance;
    // Global
    public int _FlyingSpeedDiv = 10;
    public int CurrentScore = 0;

    public int RemainBubbleShoot = 69;


    // base bubble
    public GameObject BubblePrefab;
    // Positioning 
    public Transform LoadedPosition, WaitingPosition;
    public static float LeftWall = -5.2f, RightWall = 5.2f, TopWall = 4, BotWall = -4, BotLimit = -0.2f, TopLimit = 3.0f;
    public float LowestBubblePos;
    // Objects
    Bubble _loadedBubble = null, _waitingBubble = null;

    //prefabs 
    public GameObject bubbleBoardPrefab;
    public IBubbleBoard _bubbleBoard;

    public Collider2D touchableArea;

    public Transform touchBack, touchAreaTransform;

    public Transform scoreDisplay, highScoreDisplay;

    public Transform restartBtnTransform;

    // The white bubbles for counting down Action;
    public List<GameObject> whiteBubbles;
    //GameObject[] whiteBubbles = new GameObject[6];
    public GameObject whiteBubblePrefab;

    // Arrow
    public ArrowController _shootArrow;
    public bool _usingArrow, _displayingArrow;
    bool canShowArrow;
    public bool needUpdateArrowPosition = true;

    public List<Bubble> _flyingBubbles;
    // input
    Vector3 MouseLastPosition;

    // global
    public bool gameStarted, gameEnded, gamePaused, allLoadingDone;
    public bool needUpdateStar = true, needUpdateBullet = false, needUpdateHUD = true;
    // HUD
    public GameObject HUDBG;

    public UnityEngine.UI.Text txtScore, txtHighScore;

    // Scene
    GameSceneController gameSceneController;

    void Wake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {

        gameSceneController = GameObject.FindObjectOfType<GameSceneController>();

        TopWall = Camera.main.orthographicSize - 0.5f * Bubble.BUBBLE_RADIUS;
        txtScore.text = "0";
        txtHighScore.text = PlayerPrefs.GetInt("HighScore", 0).ToString();

        restartBtnTransform.position = new Vector3(Bubble.BUBBLE_RADIUS * 8f, -(Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 1.5f), -0.01f);
        scoreDisplay.position = new Vector3(Bubble.BUBBLE_RADIUS * 5f, -(Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 1.5f), -0.01f);
        highScoreDisplay.position = new Vector3(-Bubble.BUBBLE_RADIUS * 5f, -(Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 1.5f), -0.01f);

        touchBack.position = new Vector3(0, Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 7.25f, -0.01f);
        touchAreaTransform.position = new Vector3(0, Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 7.25f, -0.01f);

        LoadedPosition.position = new Vector3(0f, Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 14f, -0.01f);
        WaitingPosition.position = new Vector3(-Bubble.BUBBLE_RADIUS * 3f, Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 15f, -0.01f);


        LeftWall = -17.5f * Bubble.BUBBLE_RADIUS / 2f;
        RightWall = 17.5f * Bubble.BUBBLE_RADIUS / 2f;

        // Load data (if not done yet)
        GlobalData.LoadLevelData();

        GameObject board = Instantiate(bubbleBoardPrefab) as GameObject;
        _bubbleBoard = board.GetComponent<IBubbleBoard>();
        // Set Data
        RemainBubbleShoot = -1;


        HUDBG.SetActive(true);


        UpdateHUD();

        gameStarted = false;
        gameEnded = false;
        allLoadingDone = false;

        
        _bubbleBoard.InitBubbleBoard();

        InitBubbles();
        _usingArrow = false;

        _shootArrow.UpdateArrow();
    }

    
    void InitBubbles()
    {
        CreateBubbleBullet();
        CreateInitWhiteBubbles();
    }

    public void CreateInitWhiteBubbles()
    {
        if (whiteBubbles != null)
        {
            whiteBubbles.Clear();
        }
        GameObject GO = Instantiate(whiteBubblePrefab) as GameObject;
        GO.transform.position = new Vector3(WaitingPosition.position.x, WaitingPosition.position.y, 0.125f);
        whiteBubbles.Add(GO);
        for (int i = 1; i < GlobalData.WHITE_COUNT; i++)
        {
            GameObject GGO = Instantiate(whiteBubblePrefab) as GameObject;
            GGO.transform.position = new Vector3(whiteBubbles[i - 1].transform.position.x + Bubble.BUBBLE_RADIUS * 1.2f, WaitingPosition.position.y, 0.125f);
            whiteBubbles.Add(GGO);
        }
    }

    public void DeleteWhiteBubble()
    {
        Destroy(whiteBubbles[GlobalData.WHITE_COUNT]);
    }

    


    void CreateBubbleBullet(int id = 0)
    {

        

        if (RemainBubbleShoot != 0)
        {
            int color = _bubbleBoard.GetRandomColor();
            if (id != 0)
                color = id;

            if (color != -1)
            {
                _waitingBubble = (Instantiate(BubblePrefab, WaitingPosition.position, WaitingPosition.rotation) as GameObject).GetComponent("Bubble") as Bubble;
                _waitingBubble.MotionDelegate = onBubbleMove;
                _waitingBubble.SetBubbleColor(color);
                _waitingBubble.gameObject.tag = "flyingbubble";
                _waitingBubble.isMoving = true;
                _waitingBubble.moveTo(WaitingPosition.position, 0.2f);

                RemainBubbleShoot--;
            }
            else
            {
                _waitingBubble = null;
            }
        }
    }

    void LoadBubbleBullet()
    {
        if (_waitingBubble == null) return;

        _loadedBubble = _waitingBubble;

        _loadedBubble.moveTo(LoadedPosition.position, 0.3f, new Vector3(0, 2.5f, 0));

        _waitingBubble = null;
        CreateBubbleBullet();

        _shootArrow.UpdateDotColor(_loadedBubble.ID);
    }

    // Update is called once per frame
    void Update()
    {
        // Update Logic
        if (gameStarted && !gameEnded && !gamePaused)
        {
            if (_waitingBubble == null)
            {
                CreateBubbleBullet();
            }
            if (_loadedBubble == null && _waitingBubble != null)
            {
                if (_waitingBubble.isMoving == false)
                {
                    LoadBubbleBullet();
                }
            }

            UpdateFlyingBubbles();

            if (needUpdateBullet) UpdateBulletBubbles();

            UpdateTouchInput();
            UpdateArrow();

        }

        // Update Loading and finish
        else if (_bubbleBoard.isDoneLoading() && !allLoadingDone)
        {
            StartCoroutine(StartTheGamePhase1());
        }

        else if (allLoadingDone && startGamePhase1Ended == false)
        {
            bool runNextPhase = false;
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                if (touchableArea.GetComponent<Collider2D>() == Physics2D.OverlapPoint(pos))
                {
                    runNextPhase = true;
                }
            }
            int i = 0;
            while (i < Input.touchCount)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                    if (touchableArea.GetComponent<Collider2D>() == Physics2D.OverlapPoint(pos))
                    {
                        runNextPhase = true;
                    }
                    break;
                }
                ++i;
            }
            if (runNextPhase)
            {
                startGamePhase1Ended = true;
                StopAllCoroutines();
                StartCoroutine(StartTheGamePhase2());
            }
        }

        // Update execution
        if (KillList.Count > 0 || DropList.Count > 0)
        {
            ExecuteBubbleLists();
        }

        UpdateHUD();

        if (_loadedBubble)
        {
            LoadedPosition.position = new Vector3(0f, Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 14f, -0.01f);
        }
        if (_waitingBubble)
        {
            WaitingPosition.position = new Vector3(-Bubble.BUBBLE_RADIUS * 3f, Camera.main.orthographicSize - Bubble.BUBBLE_RADIUS * 15f, -0.01f);
        }
        
    }

    bool startGamePhase1Ended = false;
    float camMoveTime = 0;
    IEnumerator StartTheGamePhase1()
    {
        allLoadingDone = true;
        yield return new WaitForSeconds(0.5f);

        // MOVE THE CAM
        MovingQueue mq = Camera.main.GetComponent<MovingQueue>();
        camMoveTime = _bubbleBoard.getMapData().MapSizeY * 0.15f;
        mq.moveTo(new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z), camMoveTime);

        yield return new WaitForSeconds(camMoveTime);
        startGamePhase1Ended = true;
        StartCoroutine(StartTheGamePhase2());
    }

    IEnumerator StartTheGamePhase2()
    {
        MovingQueue mq = Camera.main.GetComponent<MovingQueue>();
        mq.StopAllCoroutines();
        float newMoveTime = (camMoveTime - mq.TimeMoved) / 2.5f;
        mq.moveTo(new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z), newMoveTime);
        yield return new WaitForSeconds(newMoveTime);
        // =========
        yield return new WaitForSeconds(0.5f);
        gameStarted = true;
        CreateBubbleBullet();
    }

    void UpdateFlyingBubbles()
    {
        foreach (Bubble b in _flyingBubbles)
        {
            b.UpdateMove();
        }
    }

    void UpdateTouchInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            TouchBegan(pos);
            MouseLastPosition = pos;
            return;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (MouseLastPosition != pos)
            {
                TouchMoved(pos);
                MouseLastPosition = pos;
            }
            return;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TouchEnded(pos);
            return;
        }

        int i = 0;
        while (i < Input.touchCount)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                TouchBegan(pos);
                return;
            }
            if (Input.GetTouch(i).phase == TouchPhase.Moved)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                TouchMoved(pos);
                return;
            }
            if (Input.GetTouch(i).phase == TouchPhase.Ended)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                TouchEnded(pos);
                return;
            }
            if (Input.GetTouch(i).phase == TouchPhase.Canceled)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                return;
            }
            ++i;
        }
    }

    public void UpdateHUD()
    {
        

        txtScore.text = CurrentScore.ToString();

        if (needUpdateStar)
        {
            StarBarProcess();
            needUpdateStar = false;
        }
    }

    public UnityEngine.UI.Image Starbar;
    void StarBarProcess()
    {
        float scale = 0;

        if (scale > 1) scale = 1;


    }
      

    public void GiveBubblePoint(int p = 10)
    {
        CurrentScore += p;

        needUpdateStar = true;
        needUpdateHUD = true;
    }


    void UpdateArrow()
    {
        if (_displayingArrow)
        {
            if (needUpdateArrowPosition)
            {
                needUpdateArrowPosition = false;
                _shootArrow.UpdateArrow();
            }
            _shootArrow.gameObject.SetActive(true);
        }
        else
        {
            _shootArrow.gameObject.SetActive(false);
        }
    }

    void UpdateBulletBubbles()
    {
        if (_bubbleBoard.getBubbleList().Count <= 0) return;
        // waiting
        if (_waitingBubble != null)
        {

            bool waitingOk = false;
            if (_waitingBubble.ID == 19 || _waitingBubble.ID == 20) waitingOk = true;
            else foreach (Bubble b in _bubbleBoard.getBubbleList())
                {
                    if (_waitingBubble.ID == b.ID)
                    {
                        waitingOk = true;
                        break;
                    }
                }

            if (!waitingOk) _waitingBubble.SetBubbleColor(_bubbleBoard.GetRandomColor());
        }
        // loaded
        if (_loadedBubble != null)
        {

            bool loadedOk = false;
            if (_loadedBubble.ID == 19 || _loadedBubble.ID == 20) loadedOk = true;
            else foreach (Bubble b in _bubbleBoard.getBubbleList())
                {
                    if (_loadedBubble.ID == b.ID)
                    {
                        loadedOk = true;
                        break;
                    }
                }

            if (!loadedOk) _loadedBubble.SetBubbleColor(_bubbleBoard.GetRandomColor());
        }
        needUpdateBullet = false;

        _shootArrow.UpdateDotColor(_loadedBubble.ID);
    }

    // Handle TouchEvent
    void TouchBegan(Vector3 touch)
    {
        if (_flyingBubbles.Count > 0) return;
        if (touchableArea.GetComponent<Collider2D>() == Physics2D.OverlapPoint(touch))
        {
            _displayingArrow = true;
            var dir = touch - LoadedPosition.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _shootArrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            _usingArrow = true;

            _shootArrow.UpdateArrow();
            needUpdateArrowPosition = true;
        }
        
    }

    void TouchMoved(Vector3 touch)
    {
        if (_usingArrow)
        {
            if (touchableArea.GetComponent<Collider2D>() == Physics2D.OverlapPoint(touch))
            {
                _displayingArrow = true;
                var dir = touch - LoadedPosition.position;
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                _shootArrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                needUpdateArrowPosition = true;
            }
            else
            {
                _displayingArrow = false;
            }
        }
    }

    void TouchEnded(Vector3 touch)
    {
        _displayingArrow = false;
        if (_loadedBubble == null || _loadedBubble.isMoving || !_usingArrow)
        {

            return;
        }
        else if (touchableArea.GetComponent<Collider2D>() == Physics2D.OverlapPoint(touch))
        {

            ShootBubble(touch);
        }
        else
        {

        }
        _usingArrow = false;


    }


    // Processing
    void ShootBubble(Vector3 pos)
    {
        var dir = pos - LoadedPosition.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _shootArrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        float diffx, diffy;
        float rangle = (-angle + 90) * Mathf.PI / 180;
        diffx = Mathf.Sin(rangle);
        diffy = Mathf.Cos(rangle);
        Vector2 velocity = new Vector2(diffx / _FlyingSpeedDiv, diffy / _FlyingSpeedDiv);

        _loadedBubble.velocity = velocity;
        _loadedBubble.isMoving = true;

        _flyingBubbles.Add(_loadedBubble);

        _loadedBubble = null;


    }

    public Transform subTextRoot;
    public GameObject subBubbleTextPrefab;
    public void ReduceShootTrigger()
    {
        GameObject subt = Instantiate(subBubbleTextPrefab) as GameObject;
        subt.transform.SetParent(subTextRoot, false);

        RemainBubbleShoot -= 1;
        if (RemainBubbleShoot < 0) RemainBubbleShoot = 0;

        UpdateHUD();
    }

    public bool HasAnyChance()
    {
        return (_flyingBubbles.Count > 0 || _loadedBubble != null || _waitingBubble != null);
    }

    // Delegate
    int onBubbleMove(Vector3 position, Bubble b)
    {
        // type : 0 = Normal, 1 = excess LeftLimit, 2 = excess RightLimit , 3 =  excess TopLimit, 4 = excess BottomLimit
        if (position.y > TopWall - Bubble.BUBBLE_RADIUS / 2)
            return 3;
        if (position.y < BotWall + Bubble.BUBBLE_RADIUS / 2)
        {
            // Kill that Bitch here
            _flyingBubbles.Remove(b);
            return 4;
        }

        if (position.x < LeftWall + Bubble.BUBBLE_RADIUS / 2)
            return 1;
        if (position.x > RightWall - Bubble.BUBBLE_RADIUS / 2)
            return 2;


        return 0;
    }


    #region IndependentKiller
    public List<Bubble> KillList = new List<Bubble>();
    public List<Bubble> DropList = new List<Bubble>();

    public void AddBubbleToKillList(Bubble b)
    {
        if (!KillList.Contains(b) && !DropList.Contains(b))
            KillList.Add(b);
    }

    public void AddBubbleToDropList(Bubble b)
    {
        if (!KillList.Contains(b) && !DropList.Contains(b))
            DropList.Add(b);
    }

    public void ExecuteBubbleLists()
    {
        // kill list
        foreach (Bubble b in KillList)
        {
            StartCoroutine(b.kill());
        }

        KillList.Clear(); // finish

        // drop list
        foreach (Bubble b in DropList)
        {
            StartCoroutine(b.drop());
        }

        DropList.Clear(); // finish

    }

    #endregion

}
