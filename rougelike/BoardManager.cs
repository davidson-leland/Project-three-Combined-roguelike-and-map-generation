using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{

    [Serializable]
    public class Count
    {
        public int minimum, maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    private int columns = 8, rows = 8;
    public MapGenerator mapGenerator;
    public int additionalRooms;
    
    public int[] roomSizeMax = new int[3] { 1, 4, 10 };
    private int[] roomSizeCount = new int[3];


    public Count wallCount = new Count(5, 9);
    public Count foodCount = new Count(1, 5);

    public GameObject exit;
    public Vector3 exitLocation;
    public GameObject player;

    public GameObject[] floorTiles,
                            wallTiles,
                            foodTiles,
                            enemyTiles,
                            outerWallTiles;

    private List<Transform> boardHolder = new List<Transform>();
    private List<List<Vector3>> gridPositions = new List<List<Vector3>>();

    // private int[,] map;
    private List<Room> rooms = new List<Room>();
    private int seed = 0;

    private Queue<Entrance> entranceQ = new Queue<Entrance>();

    private Grid grid;
    private PathFinding pathFinder;

    private List<Vector3> route = new List<Vector3>();//this will hold the multple paths from one room to the next

    void InitialiseList()
    {
        gridPositions.Clear();


        for (int i = 0; i < rooms.Count; i++)
        {
            List<Vector3> tempList = new List<Vector3>();
            for (int x = 1; x < rooms[i].rows - 1; x++)
            {
                for (int y = 1; y < rooms[i].columns - 1; y++)
                {
                    if (rooms[i].map[x, y] == 0)
                    {
                        tempList.Add(new Vector3(x, y, 0f) + rooms[i].mapOffSet);
                    }
                }
            }
            gridPositions.Add(tempList);
        }

    }

    void BoardSetup()
    {
        //Debug.Log("board setup called");

        seed = 0;

        boardHolder.Clear();
        rooms.Clear();
        entranceQ.Clear();

        roomSizeCount[0] = 0;
        roomSizeCount[1] = 0;
        roomSizeCount[2] = 0;

        //if (roomSizeMax[0] > 1) roomSizeMax[0] = 1;

        mapGenerator.useRandomSeedActual = mapGenerator.useRandomSeed;

        addBoardToLists();

        while (entranceQ.Count > 0 && rooms.Count < additionalRooms + 1)
        {
            mapGenerator.useRandomSeedActual = false;
            addBoardToLists();
            

        }

        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].RemoveExcessWalls();
            instantiateBoardFromLists(i);

            //rooms[i].CreatePaths(pathFinder, grid);

        }

        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].CreatePaths(pathFinder, grid);
        }

    }
    
    void addBoardToLists()
    {
        string instr = seed == 0 ? "" : seed.ToString(), outstr;
        // int randI;

        List<int> randRoomList = new List<int>();
        List<string> seedToUse = new List<string>();

        if (roomSizeMax[0] - roomSizeCount[0] < 1 && roomSizeMax[1] - roomSizeCount[1] < 1 && roomSizeMax[2] - roomSizeCount[2] < 1)
        {
            entranceQ.Clear();
            
            return;
        }

        if (rooms.Count < 1)
        {
            int i;
            i = Random.Range(1, 3);
            
            randRoomList.Add(i);
        }
        else
        {

            if (roomSizeMax[0] - roomSizeCount[0] > 0)
            {
                randRoomList.Add(0);
            }

            if (roomSizeMax[1] - roomSizeCount[1] > 0)
            {
                randRoomList.Add(1);
            }

            if (roomSizeMax[2] - roomSizeCount[2] > 0)
            {
                randRoomList.Add(2);
            }

        }

        if (randRoomList.Count == 0)
        {
            entranceQ.Clear();
            ;
            return;
        }

        int iRand;
        iRand = Random.Range(0, randRoomList.Count);
        
        mapGenerator.recieveRoomSettings(randRoomList[iRand]);

        Room tempRoom = new Room(mapGenerator.GenerateMapForOther(instr, out outstr), 1, 1, 1, 1);



        bool isIntersecting = false;

        if (rooms.Count > 0)
        {
            Entrance tempEntrance = entranceQ.Dequeue();
            joinRooms(tempEntrance.room, tempRoom, tempEntrance.side);

            int i = 0;
            do
            {
                isIntersecting = (DoRoomsIntersect(tempRoom, rooms[i]) || DoRoomsIntersect(rooms[i], tempRoom));
                i++;

            } while (isIntersecting != true && i < rooms.Count);

            if (!isIntersecting)
            {
                tempRoom.ActuallyDrawLine();
                tempEntrance.room.ActuallyDrawLine();
            }
        }

        if (!isIntersecting)
        {
            
            boardHolder.Add(new GameObject("Board" + boardHolder.Count).transform);
            tempRoom.index = rooms.Count;
            rooms.Add(tempRoom);
            //rooms[rooms.Count].index = rooms.Count - 1;
            roomSizeCount[randRoomList[iRand]]++;

            if (rooms[rooms.Count - 1].rows * rooms[rooms.Count - 1].columns > 150)//room is atleast medium size
            {
                entranceQ.Enqueue(new Entrance(rooms[rooms.Count - 1], 0, new Vector3(0, 0, 0)));
                entranceQ.Enqueue(new Entrance(rooms[rooms.Count - 1], 1, new Vector3(0, 0, 0)));
                entranceQ.Enqueue(new Entrance(rooms[rooms.Count - 1], 2, new Vector3(0, 0, 0)));
                entranceQ.Enqueue(new Entrance(rooms[rooms.Count - 1], 3, new Vector3(0, 0, 0)));
            }

            seed += outstr.GetHashCode();
        }

    }



    bool DoRoomsIntersect(Room roomA, Room roomB)
    {
        int xMinA = (int)roomA.mapOffSet.x, xMaxA = (int)(roomA.mapOffSet.x + roomA.rows - 1),
                yMinA = (int)roomA.mapOffSet.y, yMaxA = (int)(roomA.mapOffSet.y + roomA.columns - 1),

                xMinB = (int)roomB.mapOffSet.x, xMaxB = (int)(roomB.mapOffSet.x + roomB.rows - 1),
                yMinB = (int)roomB.mapOffSet.y, yMaxB = (int)(roomB.mapOffSet.y + roomB.columns - 1);

        if (IsInside(xMinA, xMinB, xMaxB) || IsInside(xMaxA, xMinB, xMaxB))
        {
            if (IsInside(yMinA, yMinB, yMaxB) || IsInside(yMaxA, yMinB, yMaxB))
            {
                return true;
            }
        }

        return false;


    }

    bool IsInside(int inside, int min, int max)
    {
        
        if (inside >= min && inside <= max)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    void joinRooms(Room baseRoom, Room addRoom, int side)// 0 is add room to left, 1 is add room to right, 2 is add room to bottom, 3 is add room to top
    {
        int baseX, baseY, addX, addY;
        int modX, modY;

        //Debug.Log("joining rooms");
        

        switch (side)
        {
            case 0:
                baseRoom.CreateEntrance(0, addRoom, out baseX, out baseY);
                addRoom.CreateEntrance(1, baseRoom, out addX, out addY);
                modX = baseX - 1;
                modY = baseY;
                break;
            case 1:
                baseRoom.CreateEntrance(1, addRoom, out baseX, out baseY);
                addRoom.CreateEntrance(0, baseRoom, out addX, out addY);
                modX = baseX + 1;
                modY = baseY;
                break;
            case 2:
                baseRoom.CreateEntrance(2, addRoom, out baseX, out baseY);
                addRoom.CreateEntrance(3, baseRoom, out addX, out addY);
                modX = baseX;
                modY = baseY - 1;
                break;
            case 3:
                baseRoom.CreateEntrance(3, addRoom, out baseX, out baseY);
                addRoom.CreateEntrance(2, baseRoom, out addX, out addY);
                modX = baseX;
                modY = baseY + 1;
                break;
            default:
                baseRoom.CreateEntrance(1, addRoom, out baseX, out baseY);
                addRoom.CreateEntrance(0, baseRoom, out addX, out addY);
                modX = baseX + 1;
                modY = baseY;
                break;

        }

        Vector3 offSet = new Vector3(modX - addX, modY - addY, 0f);
        addRoom.mapOffSet = (offSet + baseRoom.mapOffSet);
    }

    void instantiateBoardFromLists(int intToInstantiate = 0)
    {
        rows = rooms[intToInstantiate].rows;
        columns = rooms[intToInstantiate].columns;

        
        for (int x = -0; x < rows + 0; x++)//all +/- 0's where originally +/- 1 to create the border. taking this out while testing adding multiple "rooms"
        {
            for (int y = -0; y < columns + 0; y++)
            {
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                if (/*x == -1 || x == rows || y == -1 || y == columns|| */rooms[intToInstantiate].map[x, y] == 1)
                {
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                }

                if (rooms[intToInstantiate].map[x, y] != 2)
                {
                    GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f) + rooms[intToInstantiate].mapOffSet, Quaternion.identity) as GameObject;

                    instance.transform.SetParent(boardHolder[intToInstantiate]);
                }
            }
        }

    }

    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        int randomIndex2 = Random.Range(0, gridPositions[randomIndex].Count);

        Vector3 randomPosition = gridPositions[randomIndex][randomIndex2];
        gridPositions[randomIndex].RemoveAt(randomIndex2);
        return randomPosition;
    }

    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        int objectCount = Random.Range(minimum, maximum + 1);

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = RandomPosition();
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    public void SetupScene(int level)
    {
        int bestIndexI, bestIndexJ;

        //Debug.Log("setupscene called");

        if(level != 0)
        {
            if (level % 2 == 0)
            {                
                roomSizeMax[0]++;
            }

            if(level % 3 == 0)
            {                
                roomSizeMax[1]++;
            }

            if(level % 5 == 0)
            {               
                roomSizeMax[2]++;
            }
        }

        mapGenerator = GetComponentInParent<MapGenerator>();

        if (grid == null)
        {
            grid = GetComponentInParent<Grid>();
        }


        if (pathFinder == null)
        {
            pathFinder = GetComponent<PathFinding>();
            pathFinder.SetGrid(grid);
        }
        
        BoardSetup();
        InitialiseList();

        //bestIndex = FindClosestPositionToCorner((int)Random.Range(0, 3));
        FindFurthestCorner(Random.Range(0, 3), out bestIndexI, out bestIndexJ);
        exitLocation = gridPositions[bestIndexI][bestIndexJ];
        Instantiate(exit, gridPositions[bestIndexI][bestIndexJ], Quaternion.identity);
        gridPositions[bestIndexI].RemoveAt(bestIndexJ);

        //exitLocation = GameObject.Find("Exit").transform.position;

        player = GameObject.Find("Player");

        //bestIndex = FindClosestPositionToCorner(3);

        FindClosestPositionToCorner(3, out bestIndexI, out bestIndexJ);

        Vector3 playerstart = new Vector3(gridPositions[bestIndexI][bestIndexJ].x, gridPositions[bestIndexI][bestIndexJ].y, player.transform.position.z);
        player.transform.position = playerstart;
        gridPositions[bestIndexI].RemoveAt(bestIndexJ);

        int availableSpots = 0;

        foreach (List<Vector3> inRoom in gridPositions)
        {
            availableSpots += inRoom.Count;
        }

        int wallMin = (int)(((float)wallCount.minimum / 100f) * availableSpots);
        int wallMax = (int)(((float)wallCount.maximum / 100) * availableSpots);

        int foodMin = (int)(((float)foodCount.minimum / 100) * availableSpots);
        int foodMax = (int)(((float)foodCount.maximum / 100) * availableSpots);

       // LayoutObjectAtRandom(wallTiles, wallMin, wallMax);
        LayoutObjectAtRandom(foodTiles, foodMin, foodMax);

        //int enemyCount = (int)Mathf.Log(level + adjustedRoomCount, 2f);

        int enemyCount = (roomSizeCount[0] /2 ) + roomSizeCount[1] + (roomSizeCount[2] * 2) - 3;

        if(enemyCount < 0)
        {
            enemyCount = 0;
        }

        enemyCount += (int)Mathf.Log(level, 2f);

        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);

    }

    void FindClosestPositionToCorner(int inI, out int closestIndexI, out int closestIndexJ)
    {
        Vector3 corner;
        int closestDistance = 0;

        switch (inI)
        {
            case 0:
                corner = new Vector3(0, columns, 0f);
                break;
            case 1:
                corner = new Vector3(rows, columns, 0f);
                break;
            case 2:
                corner = new Vector3(rows, 0, 0f);
                break;
            case 3:
                corner = new Vector3(0, 0, 0f);
                break;
            default:
                corner = new Vector3(0, 0, 0f);
                break;
        }

        closestIndexI = 0;
        closestIndexJ = 0;

        for (int i = 0; i < gridPositions.Count; i++)
        {
            for (int j = 0; j < gridPositions[i].Count; j++)

                if (closestDistance == 0)
                {
                    closestIndexI = i;
                    closestIndexJ = j;
                    closestDistance = (int)(Mathf.Pow(gridPositions[i][j].x - corner.x, 2) + Mathf.Pow(gridPositions[i][j].y - corner.y, 2));
                }
                else if ((int)(Mathf.Pow(gridPositions[i][j].x - corner.x, 2) + Mathf.Pow(gridPositions[i][j].y - corner.y, 2)) <= closestDistance)
                {
                    closestIndexI = i;
                    closestIndexJ = j;
                    closestDistance = (int)(Mathf.Pow(gridPositions[i][j].x - corner.x, 2) + Mathf.Pow(gridPositions[i][j].y - corner.y, 2));
                }
        }

    }

    void FindFurthestCorner(int inI, out int furthestIndexI, out int furthestIndexJ)
    {
        int compX = 0, compY = 0, furthestDistance = 0;

        furthestIndexI = 0;
        furthestIndexJ = 0;

        switch (inI)
        {
            case 0:
                compX = -1;
                compY = 1;
                break;
            case 1:
                compX = 1;
                compY = 1;
                break;
            case 2:
                compX = 1;
                compY = -1;
                break;
            case 3:
                compX = -1;
                compY = -1;
                break;
            default:
                compX = 1;
                compY = 1;
                break;
        }

        for (int i = 0; i < gridPositions.Count; i++)
        {
            for (int j = 0; j < gridPositions[i].Count; j++)
            {
                if (gridPositions[i][j].x * compX > 0 && gridPositions[i][j].y * compY > 0)
                {
                    if (furthestDistance == 0)
                    {
                        furthestIndexI = i;
                        furthestIndexJ = j;
                        furthestDistance = (int)(Mathf.Pow(gridPositions[i][j].x, 2) + Mathf.Pow(gridPositions[i][j].y, 2));
                    }
                    else if ((int)(Mathf.Pow(gridPositions[i][j].x, 2) + Mathf.Pow(gridPositions[i][j].y, 2)) >= furthestDistance)
                    {
                        furthestIndexI = i;
                        furthestIndexJ = j;
                        furthestDistance = (int)(Mathf.Pow(gridPositions[i][j].x, 2) + Mathf.Pow(gridPositions[i][j].y, 2));
                    }
                }
            }

        }
    }

    public List<Vector3> GetRoute()
    {
        return route;
    }

    /*void OnDrawGizmos()
    {
        
        Gizmos.color = Color.red;
        
        for (int i = 0; i < route.Count; i++)
        {
            Gizmos.DrawCube(route[i], new Vector3(0.5f, 0.5f, 0.5f));
            
        }
        
        foreach (Room r in rooms)
        {
            foreach (Room.Path p in r.Paths)
            {
                for (int i = 0; i < p.course.Count; i++)
                {
                    Gizmos.color = Color.blue;
                    if (  i == 0)
                    {
                        Gizmos.color = Color.red;
                    }

                    Gizmos.DrawCube(p.course[i], new Vector3(0.5f, 0.5f, 0.5f));
                }
            }
        }
        
    }*/

    
    public void FindRoute(Vector3 startPos, Vector3 endPos)
    {
        int startRoom = GetRoomForLocation(startPos);

        int endRoom = GetRoomForLocation(endPos);

        List<int> roomsToTravel = new List<int>();
        Queue<int> openSet = new Queue<int>();

       
        route.Clear();

        if(startRoom == endRoom)
        {
            actuallyCreateRoute(startRoom, startPos, endPos);

            return;
        }

        openSet.Enqueue(startRoom);

        foreach( Room r in rooms)
        {
            r.parentIndex = -1;
        }

        int t = 0;
        while(openSet.Count > 0 && t < rooms.Count)
        {
            int roomIndex = openSet.Dequeue();
            t++;

           // Debug.Log("looking for route/path");

            if (roomIndex == endRoom)
            {
                //do some cool stuff here
                bool tBool = true;
                int i = roomIndex;
                
                while (tBool == true)
                {
                    roomsToTravel.Add(i);
                    i = rooms[i].parentIndex;

                    if(i == -1)
                    {
                        tBool = false;
                    }
                }

                roomsToTravel.Reverse();

                ActuallyCreateRoute(roomsToTravel, startPos, endPos);
                

                return;
            }

            foreach(Entrance e in rooms[roomIndex].activeEntrances)
            {
                if(e.room.parentIndex == -1 && e.room !=rooms[startRoom])
                {
                    e.room.parentIndex = roomIndex;

                    openSet.Enqueue(e.room.index);
                }
            }
        }
    }

    private void actuallyCreateRoute(int roomIndex, Vector3 startPos, Vector3 endPos)
    {
        List<Vector3> tempRoute = new List<Vector3>();

        //pathFinder.logThis("start = " + startPos + ", and end = " + endPos);

        grid.CreateGrid(rooms[roomIndex]);
        tempRoute = pathFinder.FindPathForOther(startPos - rooms[roomIndex].mapOffSet, endPos - rooms[roomIndex].mapOffSet, rooms[roomIndex].mapOffSet, false, -1);

        AddToRoute(tempRoute);
    }

    public void ActuallyCreateRoute(List<int> roomsToTravel, Vector3 initialPos, Vector3 targetPos)
    {
        List<Vector3> tempRoute = new List<Vector3>();
        Vector3 startPos, endPos;
        Entrance tempEntrance;

        
        startPos = initialPos;
        tempEntrance = rooms[roomsToTravel[0]].GetEntranceIndexByRoom(rooms[roomsToTravel[1]]);
        endPos = tempEntrance.location;

       

        grid.CreateGrid(rooms[roomsToTravel[0]]);
        
        tempRoute = pathFinder.FindPathForOther(startPos , endPos, rooms[roomsToTravel[0]].mapOffSet, true, -1);


        AddToRoute(tempRoute);

        
        for (int i = 1 ; i < roomsToTravel.Count -1; i++)
        {
            int s, e;

            s = rooms[roomsToTravel[i]].GetEntranceIndexByRoom(rooms[roomsToTravel[i -1]]).side;
            e = rooms[roomsToTravel[i]].GetEntranceIndexByRoom(rooms[roomsToTravel[i + 1]]).side;

            tempRoute = rooms[roomsToTravel[i]].GetPathForDirection(s * s + e * e);

            //original code incase i need to go back to it
            //float a = Mathf.Abs( route[route.Count - 1].sqrMagnitude) - Mathf.Abs(tempRoute[tempRoute.Count - 1].sqrMagnitude);
            //float b = Mathf.Abs(route[route.Count - 1].sqrMagnitude) - Mathf.Abs(tempRoute[0].sqrMagnitude);

            //will reverse the route if needed
            float a = Mathf.Abs(route[route.Count - 1].sqrMagnitude - tempRoute[tempRoute.Count - 1].sqrMagnitude);
            float b = Mathf.Abs(route[route.Count - 1].sqrMagnitude - tempRoute[0].sqrMagnitude);

            
            if (a < b)
            {
                
                tempRoute.Reverse();
            }

            AddToRoute(tempRoute);
        }

        int c = roomsToTravel.Count -1;

        
       tempEntrance = rooms[roomsToTravel[c]].GetEntranceIndexByRoom(rooms[roomsToTravel[c - 1]]);
        
        startPos = targetPos;
        endPos = tempEntrance.location;
        
        grid.CreateGrid(rooms[roomsToTravel[c]]);
        tempRoute = pathFinder.FindPathForOther(startPos, endPos, rooms[roomsToTravel[c]].mapOffSet, true, -1);

        tempRoute.Reverse();

        AddToRoute(tempRoute);

    }

    public void AddToRoute(List<Vector3> tempRoute)
    {
        
        for (int i = 0; i < tempRoute.Count; i++)
        {
            route.Add(tempRoute[i]);
        }
    }

    public int GetRoomForLocation(Vector3 location)
    {
        int roomIndex = -1, i = 0;

        while (roomIndex == -1 && i < rooms.Count)
        {
            int minX, maxX, minY, maxY;

            minX = (int)rooms[i].mapOffSet.x;
            minY = (int)rooms[i].mapOffSet.y;

            maxX = minX + rooms[i].rows;
            maxY = minY + rooms[i].columns;
            
            if (IsInside((int)location.x,minX,maxX) && IsInside((int)location.y,minY,maxY))
            {
                //Debug.Log("found a room");
                roomIndex = i;
            }
            
            i++;
        }

        return roomIndex;
    }

    public  List<Room> GetRoomList()
    {
        return rooms;
    }

    
}


   
    public class Room
    {
        public int[,] map;
        public List<Vector3> entranceOffsetLeft, entranceOffsetRight, entranceOffsetBottom, entranceOffsetTop;

        public int rows, columns;
        public Vector3 mapOffSet = new Vector3(0, 0, 0);

        List<Vector3> line = new List<Vector3>();

        public List<Entrance> activeEntrances = new List<Entrance>();
        Entrance tempEntrance;

        Queue<int> pathsToFind = new Queue<int>();

        public struct Path
        {
            public int pathDirection;
            public List<Vector3> course;

            public Path(int _direction, List<Vector3> _course)
            {
                pathDirection = _direction;
                course = _course;
            }
        }

        public List<Path> Paths = new List<Path>();

        public List<Vector3> GetPathForDirection(int directionIndex)
        {

            int p = -1;

            for(int i = 0; i < Paths.Count && p == -1; i++)
            {
                if(Paths[i].pathDirection == directionIndex)
                {
                    p = i;
                }
            }
            return Paths[p].course;
        
        }

        public int parentIndex = -1, index;

        public Room(int[,] inMap, int entrancesLeft = 0, int entrancesRight = 0, int entrancesBottom = 0, int EntrancesTop = 0)
        {
            map = inMap;
            rows = map.GetLength(0);
            columns = map.GetLength(1);

            List<Vector3> gridPositions = new List<Vector3>();

            for (int x = 1; x < rows; x++)
            {
                for (int y = 1; y < columns; y++)
                {
                    if (map[x, y] == 0)
                    {
                        gridPositions.Add(new Vector3(x, y, 0f));
                    }
                }
            }

            if (entrancesLeft > 0)
            {
                entranceOffsetLeft = new List<Vector3>();
                int x = 0, count = 0;

                do
                {
                    int total = (rows - 1) * (columns - 1);

                    for (int y = 0; y < columns; y++)
                    {
                       
                        if (map[x, y] == 0)
                        {
                            entranceOffsetLeft.Add(new Vector3(x, y, 0f));
                        }
                        else
                        {
                            count++;
                        }
                    }
                    x++;
                } while (entranceOffsetLeft.Count < 1);

            }

            if (entrancesRight > 0)
            {
                entranceOffsetRight = new List<Vector3>();
                int x = rows - 1;

                do
                {
                    for (int y = 0; y < columns; y++)
                    {
                        if (map[x, y] == 0)
                        {
                            entranceOffsetRight.Add(new Vector3(x, y, 0f));
                        }
                    }
                    x--;
                } while (entranceOffsetRight.Count < 1);
            }

            if (entrancesBottom > 0)
            {
                entranceOffsetBottom = new List<Vector3>();
                int y = 0;

                do
                {
                    for (int x = 0; x < rows; x++)
                    {
                        if (map[x, y] == 0)
                        {
                            entranceOffsetBottom.Add(new Vector3(x, y, 0f));
                        }
                    }
                    y++;
                } while (entranceOffsetBottom.Count < 1);
            }

            if (EntrancesTop > 0)
            {
                entranceOffsetTop = new List<Vector3>();
                int y = columns - 1;

                do
                {
                    for (int x = 0; x < rows; x++)
                    {
                        if (map[x, y] == 0)
                        {
                            entranceOffsetTop.Add(new Vector3(x, y, 0f));
                        }
                    }
                    y--;
                } while (entranceOffsetTop.Count < 1);
            }

        }

        public void CreateEntrance(int side, Room otherRoom, out int outX, out int outY)
        {
            //0 is left, 1 is right, 2 is bottom, 3 is top;

            line.Clear();
            int adX = 0, adY = 0;


            switch (side)
            {
                case 0:
                    adX = -1;
                    line.Add(entranceOffsetLeft[(int)Random.Range(0, entranceOffsetLeft.Count)]);
                    // Debug.Log("creating entrance on left");
                    break;
                case 1:
                    adX = 1;
                    line.Add(entranceOffsetRight[(int)Random.Range(0, entranceOffsetRight.Count)]);
                    //Debug.Log("creating entrance on right");
                    break;
                case 2:
                    adY = -1;
                    line.Add(entranceOffsetBottom[(int)Random.Range(0, entranceOffsetBottom.Count)]);
                    break;
                case 3:
                    adY = 1;
                    line.Add(entranceOffsetTop[(int)Random.Range(0, entranceOffsetTop.Count)]);
                    break;
                default:
                    break;
            }

            outX = (int)line[0].x;
            outY = (int)line[0].y;

            //Debug.Log(outX + " " + outY);
            //Debug.Log(adX + " " + adY);
            int x = 0;//leaving this in for testing purposes, i don't want to have an infinite loop.
            do
            {
                x++;
                outX += adX;
                outY += adY;

                line.Add(new Vector3(outX, outY, 0f));


            } while (outX > 0 && outX < rows - 1 && outY > 0 && outY < columns - 1 && x < 50);

            tempEntrance = new Entrance(otherRoom, side, new Vector3(outX, outY, 0f));
        

        }

        public void ActuallyDrawLine()
        {
            for (int i = 0; i < line.Count; i++)
            {
                DrawCircle(line[i], 1);
            }

            if (activeEntrances.Count > 0)
            {
                for (int i = 0; i < activeEntrances.Count; i++)
                {
                    pathsToFind.Enqueue(activeEntrances[i].side * activeEntrances[i].side + tempEntrance.side * tempEntrance.side);
                    
                }
            }

            activeEntrances.Add(tempEntrance);        
        }

    public void CreatePaths(PathFinding pathFinder, Grid grid)
        {
            if (activeEntrances.Count == 0)
            {
                return;
            }

            while (pathsToFind.Count > 0)
            {
                Vector3 startPos, endPos;
                int path = pathsToFind.Dequeue();
                int side;

                switch (path)
                {
                    case 1:
                        startPos = GetEntranceForSide(0).location;
                        endPos = GetEntranceForSide(1).location;
                        side = 0;
                        break;
                    case 4:
                        startPos = GetEntranceForSide(0).location;
                        endPos = GetEntranceForSide(2).location;
                        side = 0;
                        break;
                    case 5:
                        startPos = GetEntranceForSide(2).location;
                        endPos = GetEntranceForSide(1).location;
                        side = 2;
                        break;
                    case 9:
                        startPos = GetEntranceForSide(0).location;
                        endPos = GetEntranceForSide(3).location;
                        side = 0;
                        break;
                    case 10:
                        startPos = GetEntranceForSide(3).location;
                        endPos = GetEntranceForSide(1).location;
                        side = 3;
                        break;
                    case 13:
                        startPos = GetEntranceForSide(2).location;
                        endPos = GetEntranceForSide(3).location;
                        side = 2;
                        break;
                    default:
                        Debug.Log("errer, path# is unsupported");
                        startPos = new Vector3(0, 0, 0);
                        endPos = new Vector3(0, 0, 0);
                        side = 0;
                        break;
                }
            
                grid.CreateGrid(this);
                Paths.Add(new Path(path, pathFinder.FindPathForOther(startPos, endPos,mapOffSet, false,side)));
            }
        }

        public Entrance GetEntranceForSide(int side)//this is sloppy and will cause problems :(
        {
            Entrance toReturn = activeEntrances[0];

            int i = 0;
            bool found = false;
            while (found == false && i < activeEntrances.Count)
            {
                if (activeEntrances[i].side == side)
                {
                    toReturn = activeEntrances[i];
                    found = true;
                }

                i++;
            }
            return toReturn;
        }

        public Entrance GetEntranceIndexByRoom(Room inRoom)
        {
            int eIndex = -1;

        if(activeEntrances.Count == 1)
        {
            return activeEntrances[0];
        }
            
            for (int i = 0; i < activeEntrances.Count && eIndex == -1; i++)
            {
                if( inRoom == activeEntrances[i].room)
                {
                    eIndex = i;
                }
            }

            return activeEntrances[eIndex];
        }

        void DrawCircle(Vector3 c, int r)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        int drawX = (int)c.x + x;
                        int drawY = (int)c.y + y;

                        if (IsInMapRange(drawX, drawY))
                        {
                            map[drawX, drawY] = 0;
                        }
                    }
                }
            }
        }

        public bool IsInMapRange(int x, int y)
        {
            return x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1);
        }

        public void RemoveExcessWalls()
        {
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < columns; y++)
                {
                    if (map[x, y] == 1)
                    {
                        int i = 0;

                        for (int xa = x - 1; xa < x + 2; xa++)
                        {

                            for (int ya = y - 1; ya < y + 2; ya++)
                            {
                                if (xa >= 0 && xa < rows && ya >= 0 && ya < columns)
                                {
                                    if (map[xa, ya] == 0)
                                    {
                                        i++;
                                    }
                                }
                            }

                        }
                        if (i == 0)
                        {
                            map[x, y] = 2;
                        }
                    }
                }
            }
        }
    }

    
    public struct Entrance
    {
        public Room room;
        public int side;
        public Vector3 location;

        public Entrance(Room inRoom, int inSide, Vector3 _location)
        {
            room = inRoom;
            side = inSide;
            location = _location;
        }

    }


