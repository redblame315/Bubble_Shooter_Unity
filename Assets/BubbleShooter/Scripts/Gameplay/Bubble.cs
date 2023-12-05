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
using System;
using System.Collections.Generic;

public class Bubble : MonoBehaviour
{

    const float POP_WAIT = 1f;

    public GameObject TheBubble; // Normal Bubble
    public GameObject TheBlankBubble;

    [NonSerialized]
    public GameObject ShowingPart; // Bubble's part being using

    GameObject Frosty;
    public GameObject FrostyPrefab;

    // different color
    public Sprite[] BubbleSprites, BlankBubbleSprites;

    // bubble's id
    public int ID = 0, special = 0;
    public int counterID = 0;
    public int _positionX = -1, _positionY = -1; // Position in parent

    public int KillID = -1;
    /* Determine what to do next frame
     * 0 = Killed by Matchs
     */
    public bool isFreezed = false;
    public bool markedToKill = false;
    // Linked
    public List<Bubble> linkedBubbles;
    GameplayController gameplayController;
    // Movement
    public float angle; // -90 .. 90  The trayectory angle ___\___
    public bool isMoving;
    public Vector2 velocity = new Vector2(0, 0);

    /* Constants */
    private const float _killSpeed = 10.0f;
    public static float BUBBLE_RADIUS = 0.425f, BUBBLE_COLLIDE_RADIUS = 0.25f;

    /*
	 * Delegates
	 */
    MotionDetectionDelegate motionDelegate;
    public delegate int MotionDetectionDelegate(Vector3 position, Bubble b);
    public delegate void CollisionDetectionDelegate(GameObject bubble);


    public MotionDetectionDelegate MotionDelegate
    {
        set
        {
            motionDelegate = value;
        }
    }

    // Reset Object
    public void ResetBubbleProperties()
    {
        ID = 0; special = 0;
        counterID = 0;
        _positionX = -1; _positionY = -1; // Position in parent

        KillID = -1;

        isFreezed = false;
        markedToKill = false;

        linkedBubbles.Clear();
    }


    public void SetBubbleColor(int id, bool animateTouch = false)
    {
        if (ShowingPart != null)
        {
            DestroyImmediate(ShowingPart);
        }

        try
        {
            // Set new ID
            special = 0;
            ID = id;
            SpriteRenderer srenderer = TheBubble.GetComponent<SpriteRenderer>();
            srenderer.sprite = BubbleSprites[ID - 1];

            ShowingPart = Instantiate(TheBubble) as GameObject;
            ShowingPart.transform.parent = transform;
            ShowingPart.transform.localPosition = TheBubble.transform.position;
            ShowingPart.name = TheBubble.name;

            if (animateTouch)
            {
                // Set Animation Default Frame
                Animator animator = ShowingPart.GetComponent<Animator>();
                animator.enabled = true;

            }
            SetPartVisible();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            Debug.Log(ID);
        }
    }

    void SetPartVisible()
    {
        ShowingPart.transform.localScale = new Vector3(1, 1, 1);
    }

    // Use this for initialization
    void Start()
    {
        isMoving = false;

        gameplayController = GameObject.FindObjectOfType<GameplayController>();
    }

    // Update is called once per frame
    public void UpdateMove()
    {
        if (isMoving)
        {
            // Update Flying
            transform.position = new Vector3(transform.position.x + velocity.x, transform.position.y, transform.position.z);

            // type : 0 = Normal, 1 = excess LeftLimit, 2 = excess RightLimit , 3 =  excess TopLimit, 4 = excess BottomLimit
            if (motionDelegate != null)
            {
                int crashdir = motionDelegate(transform.position, this);
                switch (crashdir)
                {
                    case 1:
                        velocity.x = -velocity.x;
                        transform.position = new Vector3(GameplayController.LeftWall + BUBBLE_RADIUS / 2, transform.position.y, transform.position.z);
                        break;
                    case 2:
                        velocity.x = -velocity.x;
                        transform.position = new Vector3(GameplayController.RightWall - BUBBLE_RADIUS / 2, transform.position.y, transform.position.z);
                        break;
                }
            }

            // Update Flying
            transform.position = new Vector3(transform.position.x, transform.position.y + velocity.y, transform.position.z);

            // type : 0 = Normal, 1 = excess LeftLimit, 2 = excess RightLimit , 3 =  excess TopLimit, 4 = excess BottomLimit
            if (motionDelegate != null)
            {
                int crashdir = motionDelegate(transform.position, this);
                switch (crashdir)
                {
                    case 4:
                        kill();
                        break;
                }
            }
        }
    }
    public void UpdateMove1()
    {
        if (isMoving)
        {
            // Update Flying
            GetComponent<Rigidbody2D>().MovePosition(new Vector3(transform.position.x + velocity.x, transform.position.y + velocity.y, transform.position.z));

            // type : 0 = Normal, 1 = excess LeftLimit, 2 = excess RightLimit , 3 =  excess TopLimit, 4 = excess BottomLimit
            if (motionDelegate != null)
            {
                int crashdir = motionDelegate(transform.position, this);
                switch (crashdir)
                {
                    case 1:
                        velocity.x = -velocity.x;
                        GetComponent<Rigidbody2D>().MovePosition(new Vector3(GameplayController.LeftWall + BUBBLE_RADIUS / 2, transform.position.y, transform.position.z));
                        break;
                    case 2:
                        velocity.x = -velocity.x;
                        GetComponent<Rigidbody2D>().MovePosition(new Vector3(GameplayController.RightWall - BUBBLE_RADIUS / 2, transform.position.y, transform.position.z));
                        break;
                }
            }

            // type : 0 = Normal, 1 = excess LeftLimit, 2 = excess RightLimit , 3 =  excess TopLimit, 4 = excess BottomLimit
            if (motionDelegate != null)
            {
                int crashdir = motionDelegate(transform.position, this);
                switch (crashdir)
                {
                    case 4:
                        kill();
                        break;
                }
            }
        }
    }

    public void DisconnectBubble(bool isKilled = false)
    {
        // Disconnecting
        foreach (Bubble b in this.linkedBubbles)
        {
            if (b == null) continue;
            if (b.linkedBubbles.Contains(this))
            {
                b.linkedBubbles.Remove(this);
            }
            //if (linked_bubble == null) continue;
            if (isKilled)
                if (b.isFreezed == true)
                {
                    b.UnFreeze();
                }

        }
    }

    public void UnFreeze()
    {
        if (isFreezed)
        {
            Animator animator = Frosty.gameObject.GetComponent<Animator>();
            animator.enabled = true;
            animator.SetTrigger("Die");
            isFreezed = false;
        }
    }

    // For delay only
    public Bubble rootDelay = null;
    public int dieDelayCount = 0;
    public IEnumerator kill()
    {

        this.isMoving = false;
        this.markedToKill = true;

        yield return new WaitForSeconds(dieDelayCount * 0.05f);

        if (special == 1)
        {
            var children = new List<GameObject>();
            foreach (Transform child in ShowingPart.transform) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));

        }
        else if (special == 3)
        {
            gameplayController.ReduceShootTrigger();
        }


        Animator animator = ShowingPart.GetComponent<Animator>();
        animator.enabled = true;

        if (ID == 20)
            animator.Play("BombBroken");
        else
            animator.Play("Broken");

        var childs = new List<GameObject>();
        foreach (Transform child in ShowingPart.transform) childs.Add(child.gameObject);
        childs.ForEach(child => Destroy(child));

        yield return new WaitForSeconds(POP_WAIT);

        Destroy(ShowingPart.GetComponent<Renderer>());
        Destroy(this.GetComponent<Collider2D>());
        Destroy(gameObject, 1);

        gameplayController.GiveBubblePoint();


    }

    public IEnumerator drop()
    {
        this.isMoving = false;
        this.markedToKill = true;
        // effect
        this.transform.parent = null;
        this.GetComponent<Collider2D>().enabled = true;
        this.GetComponent<Rigidbody2D>().isKinematic = false;
        this.GetComponent<Rigidbody2D>().AddForce(new Vector2(UnityEngine.Random.Range(-50f, 50f), 180f));

        if (special == 1)
        {
            var children = new List<GameObject>();
            foreach (Transform child in ShowingPart.transform) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));

        }

        yield return new WaitForSeconds(1.0f);
        this.GetComponent<Rigidbody2D>().isKinematic = true;

        Animator animator = ShowingPart.GetComponent<Animator>();
        animator.enabled = true;
        if (ID == 20)
            animator.Play("BombBroken");
        else
            animator.Play("Broken");

        var childs = new List<GameObject>();
        foreach (Transform child in ShowingPart.transform) childs.Add(child.gameObject);
        childs.ForEach(child => Destroy(child));

        if (this.isFreezed)
        {
            Frosty.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(POP_WAIT);

        Destroy(ShowingPart.GetComponent<Renderer>());
        Destroy(this.GetComponent<Collider2D>());
        Destroy(gameObject);

        gameplayController.GiveBubblePoint();
    }

    public IEnumerator shootUp(int point = 0)
    {
        this.isMoving = false;

        // effect
        this.transform.parent = null;
        this.GetComponent<Collider2D>().enabled = true;
        this.GetComponent<Rigidbody2D>().isKinematic = false;

        this.GetComponent<Rigidbody2D>().AddForce(new Vector2(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(300f, 400f)));


        yield return new WaitForSeconds(1.0f);
        this.GetComponent<Rigidbody2D>().isKinematic = true;

        Animator animator = ShowingPart.GetComponent<Animator>();
        animator.enabled = true;

        if (ID == 20)
            animator.Play("BombBroken");
        else
            animator.Play("Broken");

        yield return new WaitForSeconds(POP_WAIT);

        Destroy(ShowingPart.GetComponent<Renderer>());
        Destroy(this.GetComponent<Collider2D>());
        Destroy(gameObject);

        gameplayController.GiveBubblePoint(point);
    }

    public void moveTo(Vector3 destination, float duration, Vector3 manipulate = new Vector3())
    {
        StopAllCoroutines();
        StartCoroutine(tweenTo(destination, duration, manipulate));
    }

    IEnumerator tweenTo(Vector3 destination, float duration, Vector3 manipulate = new Vector3())
    {
        isMoving = true;
        float timeThrough = 0.0f;
        Vector3 initialPosition = transform.position;
        while (Vector3.Distance(transform.position, destination) >= 0.05 && timeThrough < duration)
        {
            timeThrough += Time.deltaTime;
            Vector3 target = Vector3.Lerp(initialPosition, destination, timeThrough / duration);
            Vector3 manip = Vector3.Lerp(Vector3.zero, manipulate, Mathf.PingPong(timeThrough, duration / 2));
            transform.position = target + manip;
            yield return null;
        }
        transform.position = destination;
        isMoving = false;
    }

    IEnumerator scaleTo(Vector3 scale, float duration)
    {
        float timeThrough = 0.0f;

        Vector3 initialScale = transform.localScale;

        while (transform.localScale.x >= 0.1)
        {
            timeThrough += Time.deltaTime;
            Vector3 target = Vector3.Lerp(initialScale, scale, timeThrough / duration);
            transform.localScale = target;
            yield return null;
        }
    }

}
