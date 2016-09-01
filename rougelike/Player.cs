using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
//using UnityEngine.SceneManagement;

public class Player : MovingObject
{
    public Camera gameCamera;
    public int wallDamage = 1;

    public int pointsPerFood = 10;
    public int pointsperSoda = 20;

    public float restartLevelDelay = 1f;

    public Text foodText;

    public AudioClip    moveSound1,
                        moveSound2,
                        eatSound1,
                        eatSound2,
                        drinkSound1,
                        drinkSound2,
                        gameOverSound;


    private Animator animator;
    private Vector3 offSet = new Vector3(1, 1, 0);

    private int food, lastH = 0, lastV = 0;


    protected override void Start()
    {
        animator = GetComponent<Animator>();

        food = GameManager.instance.playerFoodPoints;
        foodText.text = "Food: " + food;

        pauseMovement = false;

        base.Start();

    }

    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = food;
    }

    // Update is called once per frame
    void Update()
    {

        offSet = transform.position;
        offSet.z = -10f;

        gameCamera.transform.position = offSet;

        //if (!GameManager.instance.playersTurn) return;

        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0)
        {
            vertical = 0;
        }

        if ((lastH != 0 && horizontal == 0) || (lastV != 0 && vertical == 0))
        {
            doMoving = false;
        }

        if (((horizontal != 0 && lastH == 0) || (lastV == 0 && vertical != 0)) && !moving && !followingPath)
        {
            doMoving = true;
            AttemptMove<Wall>(horizontal, vertical);
        }

        lastH = horizontal;
        lastV = vertical;

        if (Input.GetMouseButtonDown(1) && !followingPath)
        {
            //Debug.Log(GameManager.instance.boardScript.GetRoomForLocation(transform.position));
            InitiatePathFollwing(GameManager.instance.FindRouteToFinish());
        }

        //Debug.Log(doMoving);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Exit")
        {
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        else if (other.tag == "Food")
        {
            food += pointsPerFood;
            foodText.text = "+" + pointsPerFood + " Food: " + food;
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        }
        else if(other.tag == "Soda")
        {
            food += pointsperSoda;
            foodText.text = "+" + pointsperSoda + " Food: " + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }
    }

    protected override void OnCantMove <T> (T component)
    {
        Wall hitwall = component as Wall;
        hitwall.DamageWall(wallDamage);
        animator.SetTrigger("playerChop");
    }

    private void Restart()
    {
        Application.LoadLevel(Application.loadedLevel);

        //SceneManager.LoadScene("Main");
    }

    public void LoseFood(int loss)
    {
        animator.SetTrigger("playerHit");
        food -= loss;
        foodText.text = "-" + loss + " Food: " + food;
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if(food <= 0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.GameOver();
        }
    }

    protected override void AttemptMove <T> (int xDir, int yDir)
    {
        //food--;
        //foodText.text = "Food: " + food;

        base.AttemptMove<T>(xDir, yDir);

        RaycastHit2D hit;

        if(Move(xDir,yDir,out hit))
        {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);

        }

        //CheckIfGameOver();

        //GameManager.instance.playersTurn = false;

        //NotifyPassedSquare();
    }

     protected override bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        setBoxCollider(false);
        hit = Physics2D.Linecast(start, end, blockingLayer);
        setBoxCollider(true);

        if (hit.transform == null && !moving)
        {
            //Debug.Log("x = " + xDir + ", y = " + yDir);
            StartCoroutine(SmoothMovement(end,xDir,yDir));
            return true;
        }

        return false;
    }

    protected override void ContinueOnPath()
    {
        int xDir = 0, yDir = 0;

        if(pathTarget.x == transform.position.x)
        {
            yDir = pathTarget.y > transform.position.y ? 1 : -1;
        }
        else
        {
            xDir = pathTarget.x > transform.position.x ? 1 : -1;
        }
        doMoving = true;
        AttemptMove<Wall>(xDir, yDir);
    }
    

    protected override void NotifyPassedSquare(bool noMove = false)
    {
        base.NotifyPassedSquare();

        food--;
        foodText.text = "Food: " + food;

        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
    }
}
