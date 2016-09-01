using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MovingObject : MonoBehaviour {

    public float moveTime = 0.1f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    protected float inverseMoveTime;
    public bool moving, doMoving = false;

    protected Queue<Vector3> path = new Queue<Vector3>();
    protected bool followingPath = false;
    protected Vector3 pathTarget;

    protected bool pauseMovement = true;
    public Vector3 nodeLocation;

    // Use this for initialization
    protected virtual void Start () {

        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
	}

    protected RaycastHit2D DetectCollision(Vector2 start, Vector2 end, LayerMask aLayer)
    {
        RaycastHit2D hit;
        setBoxCollider(false);
        hit = Physics2D.Linecast(start, end, blockingLayer);
        setBoxCollider(true);

        return hit;
    }

    protected virtual bool Move (int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);
        
        hit = DetectCollision(start, end, blockingLayer);
        

        if(hit.transform == null && !moving)
        {
            StartCoroutine(SmoothMovement(end,xDir, yDir));
            return true;
        }

        return false;
    }

    bool HasPassedEnd(Vector3 end, int dirX, int dirY)
    {
        //Debug.Log("has passed end calld");
        
        if (dirX != 0)
        {
            return ((transform.position.x - end.x) * dirX > 0);
        }

        return ((transform.position.y - end.y) * dirY > 0);

    }

    public Vector3 GetNodeLocation(Vector3 inVector)
    {
        inVector.x = Mathf.Round(inVector.x);
        inVector.y = Mathf.Round(inVector.y);

        return inVector;
    }

    protected IEnumerator SmoothMovement(Vector3 end, int dirX = 0, int dirY = 0)
    {
        //Debug.Log("smooth movement called");
        while (pauseMovement)
        {
            //Debug.Log("pausing mooving");
            yield return null;
        }

        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        
        Vector2 start = transform.position;
        Vector2 farEnd = new Vector2(end.x, end.y) + new Vector2(dirX, dirY);

        RaycastHit2D hit = DetectCollision(end, farEnd, blockingLayer);

        if (hit.transform != null || end == pathTarget)
        {
            doMoving = false;
        }

        
        while (doMoving)
        {
            if (!pauseMovement)
            {
                if (HasPassedEnd(end, dirX, dirY))
                {
                    // Debug.Log("has passed end");
                    end.x += dirX;
                    end.y += dirY;
                    //check for collisions and other fun stuff.

                    NotifyPassedSquare();

                    start = transform.position;
                    farEnd = new Vector2(end.x, end.y) + new Vector2(dirX, dirY);
                    hit = DetectCollision(end, farEnd, blockingLayer);

                    

                    if (hit.transform != null)
                    {
                        
                        doMoving = false;

                    }

                    if (end == pathTarget && followingPath)
                    {
                        doMoving = false;
                    }
                }

                Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end + new Vector3(dirX, dirY, 0), inverseMoveTime * Time.deltaTime);
                rb2D.MovePosition(newPosition);
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                moving = true;

                nodeLocation = GetNodeLocation(transform.position);
                yield return null;
            }
            else
            {
                yield return null;
            }
            
        }

        //Debug.Log("ending movement");

        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            moving = true;

            nodeLocation = GetNodeLocation(transform.position);
            yield return null;
        }

        moving = false;
        
        OnSmoothMoveEnd();
        //Debug.Log("movement ending");

    }

    protected virtual void OnSmoothMoveEnd()
    {
        //Debug.Log("on Smooth move end called");
        if (path.Count != 0)
        {

            if(transform.position == pathTarget)
            {
                pathTarget = path.Dequeue();
                
            }

            ContinueOnPath();
        }
        else
        {
            followingPath = false;
        }
    }

    protected abstract void ContinueOnPath();

    protected virtual void NotifyPassedSquare(bool noMove = false)
    {
        //Debug.Log("notify passedsquare");
    }

    protected virtual void AttemptMove <T> (int xDir, int yDir) where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

       
        if (hit.transform == null)
        {
            NotifyPassedSquare(true);// adding optional bool was a messy solution to an ininite loop problem in the enemy class IMO
            
            return;
        }
        

        T hitComponent = hit.transform.GetComponent<T>();

        if(!canMove && hitComponent != null)
        {
            OnCantMove(hitComponent);
        }
    }

    protected void setBoxCollider( bool collide)
    {
        boxCollider.enabled = collide;
    }

    protected abstract void OnCantMove<T>(T component)
        where T : Component;


    void CreatePathQueue(List<Vector3> inList)
    {
        path.Clear();
        for (int i = 0; i < inList.Count; i++)
        {
            path.Enqueue(inList[i]);
        }
    }

    protected int vDistance(Vector3 vectorOne, Vector3 vectorTwo)
    {
        
        float x = (int)vectorOne.x - (int)vectorTwo.x;
        float y = (int)vectorOne.y - (int)vectorTwo.y;
        
        int toreturn = (int)new Vector3(x, y, 0).magnitude;
        
        return toreturn;
    }

    public virtual void InitiatePathFollwing(List<Vector3> inList)
    {
        //Debug.Log("init path following called");

        CreatePathQueue(inList);

        followingPath = true;

        pathTarget = path.Dequeue();

        if(path.Count != 0)
        {
            pathTarget = path.Dequeue();
        }

        if (moving)
        {
            return;
        }

        doMoving = true;
        ContinueOnPath();
    }

    public void logThis(string toLog)//use to debug crashing functions
    {
        Debug.Log(toLog);
    }

}
