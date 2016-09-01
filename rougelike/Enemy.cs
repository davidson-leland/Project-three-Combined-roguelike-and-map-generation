using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Enemy : MovingObject {

    public int playerDamage;

    public AudioClip enemyAttack1, EnemyAttack2;

    private Animator animator;
    private Transform target;
    private bool skipMove;

    protected bool isAggrod = false;
    public int aggroRadius = 5;

    protected Player player;

    protected bool canMelee = true;

    private int breakCheckNumber;

    protected override void Start()
    {
        GameManager.instance.AddEnemyToList(this);
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        
        base.Start();

        InitRoaming();

        PathFindingBreakCheck();
    }

    private void PathFindingBreakCheck()//watches to see if pathfind broke and will put ai back on roaming mode
    {
        if(!moving && !pauseMovement)
        {
            breakCheckNumber++;

            if(breakCheckNumber >= 3)
            {
                breakCheckNumber = 0;

                path.Clear();
                InitRoaming();
            }
        }

        Invoke("PathFindingBreakCheck", 1);
    }
	
    protected override void AttemptMove <T> (int xDir, int yDir)
    {
        
        base.AttemptMove<T>(xDir, yDir);
    }

    void Update()
    {
        pauseMovement = !player.moving;
        //Debug.Log(pauseMovement);
    }


    public void MoveEnemy()
    {
        int xDir = 0, yDir = 0;

        if(Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon)
        {
            yDir = target.position.y > transform.position.y ? 1 : -1;
        }
        else
        {
            xDir = target.position.x > transform.position.x ? 1 : -1;
        }

        AttemptMove<Player>(xDir, yDir);
    }

    protected override void OnSmoothMoveEnd()
    {

        if (isAggrod)
        {

            FindPathToPlayer();
            return;
         }

        base.OnSmoothMoveEnd();

        if (path.Count == 0 && !followingPath)
        {

            if (isAggrod)
            {
                
                FindPathToPlayer();
            }
            else
            {
                InitRoaming();
            }

            
        }

    }

    protected override void NotifyPassedSquare(bool noMove = false)
    {
        
        breakCheckNumber = 0;
        //Debug.Log("notify passed square");
        base.NotifyPassedSquare();

        if(nodeLocation == player.nodeLocation)
        {
            //AttackEnded();
            return;
        }
        
        if (CheckAggro() && !noMove)
        {
            
            FindPathToPlayer();
        }
    }

    protected void FindPathToPlayer()
    {
        doMoving = false;
        path.Clear();
        //MoveEnemy();
        doMoving = false;

        List<Room> roomList = GameManager.instance.boardScript.GetRoomList();


        int roomIndex = roomList[GameManager.instance.boardScript.GetRoomForLocation(player.transform.position)].index;


        GameManager.instance.boardScript.FindRoute(transform.position, player.transform.position );

        //doMoving = true;
        InitiatePathFollwing(GameManager.instance.boardScript.GetRoute());
    }

    protected bool CheckAggro()
    {
        int distanceToPlayer = vDistance(nodeLocation, player.nodeLocation);
        

        if (isAggrod)
        {
            if (distanceToPlayer <= aggroRadius * 3)
            {
                
                if(distanceToPlayer <= 1 && canMelee)
                {
                    OnCantMove<Player>(player);
                    return true;
                }
                
                //Debug.Log("is still aggroed");
                isAggrod = true;
                return true;
            }
            // Debug.Log("de aggro");
            isAggrod = false;
            return false;
            
        }
        else
        {
            if (distanceToPlayer <= aggroRadius)
            {
                //Debug.Log("init aggro");
                isAggrod = true;
                return true;
            }
        }
        isAggrod = false;

       // Debug.Log("is not aggrod");
        return false;
    }

    protected override void ContinueOnPath()
    {
        //Debug.Log("continue on path called");
        
        int xDir = 0, yDir = 0;

        if (pathTarget.x == transform.position.x)
        {
            yDir = pathTarget.y > transform.position.y ? 1 : -1;
        }
        else
        {
            xDir = pathTarget.x > transform.position.x ? 1 : -1;
        }

        doMoving = true;
        AttemptMove<Player>(xDir, yDir);
    }

    protected override void OnCantMove<T>(T component)
    {
        //Debug.Log("on cant move called");
        if (canMelee)
        {
            meleeAttack<T>(component);
        }
    }

    protected void meleeAttack<T>(T component)
    {
        //Debug.Log("attacking player");
        Player hitPlayer = component as Player;
        animator.SetTrigger("enemyAttack");
        SoundManager.instance.RandomizeSfx(enemyAttack1, EnemyAttack2);


        hitPlayer.LoseFood(playerDamage);

        canMelee = false;
        Invoke("allowMelee", 1);

        path.Clear();

        doMoving = false;

        AttackEnded();
    }

    protected void AttackEnded()
    {
        if (!moving)
        {
            int distanceToPlayer = vDistance(nodeLocation, player.nodeLocation);

            if (distanceToPlayer < 2)
            {
                Invoke("AttackEnded", 1);

                if (canMelee && player.moving)
                {
                    meleeAttack(player);
                }
                return;
            }

            path.Clear();
            InitRoaming();
        }
        
    }

    protected void allowMelee()
    {
        canMelee = true;
    }

    public void InitRoaming()
    {
       // Debug.Log(" init roaming");

        if(nodeLocation == player.nodeLocation)
        {
            Invoke("InitRoaming", 1.0f);
            return;
        }

       
        List<Room> roomList = GameManager.instance.boardScript.GetRoomList();
            

         int avoid = roomList[GameManager.instance.boardScript.GetRoomForLocation(transform.position)].index;

        int randomIndex;// = Random.Range(0, roomList.Count);

        do
        {
            randomIndex = Random.Range(0, roomList.Count);

        } while (randomIndex == avoid);

        Vector3 goalVector = new Vector3(0, 0, 0);

        goalVector.x = roomList[randomIndex].rows / 2;

        goalVector.y = roomList[randomIndex].columns / 2;

        if(roomList[randomIndex].map[(int)goalVector.x,(int)goalVector.y] != 0)
        {
            goalVector.x = 1;

            while (roomList[randomIndex].map[(int)goalVector.x, (int)goalVector.y] != 0)
            {
                goalVector.x++;
            }
        }

        goalVector += roomList[randomIndex].mapOffSet;

        GameManager.instance.boardScript.FindRoute(transform.position, goalVector);
        
        InitiatePathFollwing(GameManager.instance.boardScript.GetRoute());

    }
}
