using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public float levelStartDelay = 2f;
    public float turnDelay = 0.1f;
    public static GameManager instance = null;
    public BoardManager boardScript;
    public MapGenerator mapScript;
    public int playerFoodPoints = 100;
    [HideInInspector] public bool playersTurn = true;


    private Text levelText;
    private GameObject levelImage;
    private int level  = 1;
    private List<Enemy> enemies;
    private bool enemiesMoving;
    private bool doingSetup;

	// Use this for initialization
	void Awake () {

        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();
        mapScript = GetComponent<MapGenerator>();

        if (boardScript.player == null)
        {
            boardScript.player = GameObject.Find("Player");
        }
        InitGame();	
	}

    private void OnLevelWasLoaded (int index)
    {
        //Debug.Log("onlevelwasloaded called");

        level++;

        InitGame();

    }

    void InitGame()
    {
        //Debug.Log("initgame called");
        doingSetup = true;

        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();

        levelText.text = "Day " + level;
        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();
        boardScript.SetupScene(level);
    }

    private void HideLevelImage()
    {
        //Debug.Log("hiding level image");
        levelImage.SetActive(false);
        doingSetup = false;

    }

	
    public void GameOver()
    {
        levelText.text = "After " + level + " days, you starved.";
        levelImage.SetActive(true);
        enabled = false;
    }

	// Update is called once per frame
	void Update () {
	
        if(playersTurn || enemiesMoving || doingSetup)
        {
            return;
        }

        //StartCoroutine(MoveEnemies());


        if (Input.GetMouseButtonDown(1) )
        {
           for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].InitRoaming();


            }
        }

    }

    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
    }

    public List<Vector3> FindRouteToFinish()
    {
        boardScript.FindRoute(boardScript.player.transform.position, boardScript.exitLocation);

        return boardScript.GetRoute();
    }

    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay);
        if (enemies.Count == 0)
        {
            yield return new WaitForSeconds(turnDelay);
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;
        enemiesMoving = false;
    }


}

