using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class PathFinding : MonoBehaviour {

    Grid grid;

    public Transform seeker, target;

    void Awake()
    {
        if(grid == null)
        {
            grid = GetComponent<Grid>();
            
        }
        
    }

    public void SetGrid(Grid _grid)
    {
        grid = _grid;
    }

    /*public PathFinding(Grid _grid)
    {
        grid = _grid;
    }

    public PathFinding()
    {

    }*/

    void Update()
    {
        /* if (Input.GetMouseButtonDown(1)){

           FindPath(seeker.position, target.position);
         }*/

        //FindPath(seeker.position, target.position);
    }

    public List<Vector3> FindPathForOther(Vector3 startPos, Vector3 targetPos, Vector3 offSet, bool useOffSet = false, int side = 0)
    {
        
        List<Vector3> outPath = new List<Vector3>();

        outPath.Add(startPos + offSet);


        if (useOffSet)
        {
            startPos  -= offSet;
            //targetPos += offSet;
            
        }

        if(startPos == targetPos)
        {
            return outPath;
        }

        FindPath(startPos, targetPos);
        
        for(int i = 0; i < grid.path.Count; i++)
        {
            outPath.Add(grid.path[i].worldPosition);
        }

        //outPath.Add(targetPos);

        if (useOffSet)
        {
            outPath[0] -= offSet;
        }

        outPath =  RemoveDiagonals(outPath, side, offSet);

        outPath = OptimizePath(outPath);


        return outPath;
        
    }

    private List<Vector3> OptimizePath(List<Vector3> inList)
    {

        for(int i =1; i < inList.Count -1; i++)
        {
            if( inList[i -1].x == inList[i+1].x || inList[i - 1].y == inList[i + 1].y)
            {
                inList.Remove(inList[i]);
                i--;
            }
        }

        return inList;


    }

    public void logThis( string toLog)//use to debug crashing functions
    {
        UnityEngine.Debug.Log(toLog);
    }

     List<Vector3> RemoveDiagonals( List<Vector3> inList, int side, Vector3 offSet)
    {
        //UnityEngine.Debug.Log("+++++++++++++++ starting to remove diags +++++++++++++++++++++++");
        

        for ( int iP = 0; iP < inList.Count; iP++)
        {
            inList[iP] -= offSet;
        }

        for ( int i = 0; i < inList.Count - 1; i++)
        {
            if( inList[i].x != inList[i + 1].x && inList[i].y != inList[i + 1].y)
            {
                Vector3 newDir = new Vector3(0, 0, 0);
                Vector3 returnDir = new Vector3(0, 0, 0);

                if ( i == 0)
                {
                    switch (side)
                    {
                        case 0:
                            newDir.x = 1;
                            break;
                        case 1:
                            newDir.x = -1;
                            break;
                        case 2:
                            newDir.y = 1;
                            break;
                        case 3:
                            newDir.y = -1;
                            break;
                        default:
                            newDir = inList[i + 1] - inList[i];

                            Vector3 tempV = inList[i];

                            tempV.x += newDir.x;
                          
                            if (grid.getNodeFromWorldPosition(tempV).walkable)
                            {
                                newDir.y = 0;
                            }
                            else
                            {
                                newDir.x = 0;
                            }

                            
                            break;
                    }
                }
                else
                {
                    newDir = inList[i] - inList[i - 1];
                }
                
                returnDir = inList[i + 1] - inList[i] - newDir;

                if (!grid.getNodeFromWorldPosition(inList[i] + newDir).walkable)
                {
                    Vector3 tempV = newDir;
                    newDir = returnDir;
                    returnDir = tempV;
                }

                Vector3 cornerVector = new Vector3(0, 0, 0);
                Vector3 returnCornerVector = new Vector3(0, 0, 0);
                Vector3 compVector = new Vector3(0, 0, 0);
                Vector3 returnCompVector = new Vector3(0, 0, 0);

                cornerVector = inList[i];
                returnCornerVector = cornerVector;

                compVector = inList[i] + newDir;
                

                while (grid.getNodeFromWorldPosition(compVector).walkable)
                {
                    returnCompVector = compVector;

                    while (grid.getNodeFromWorldPosition(returnCompVector).walkable)
                    {

                        if (inList.Contains(returnCompVector))
                        {
                            cornerVector = compVector;
                            returnCornerVector = returnCompVector;
                            break;
                        }

                        returnCompVector += returnDir;

                        if(!grid.ConTainsCheck(returnCompVector))
                        {
                            break;
                        }
                    }

                    compVector += newDir;

                    if (cornerVector != inList[i] && cornerVector == returnCornerVector)//returned to line without haveing turn a corner.
                    {
                        break;
                    }

                    if (!grid.ConTainsCheck( compVector))//reached end of map
                    {
                        
                        break;
                    }

                }

                
               
                int endCutIndex = inList.FindIndex(x => x == returnCornerVector);
                int startCutIndex = i + 1;
                
                List<Vector3> tempList = new List<Vector3>();

                for(int tI = 0; tI < startCutIndex; tI++)//put in optimization here. eliminate vectors from list that are going the same direction as previous
                {
                    tempList.Add(inList[tI]);

                   
                }
                
                compVector = inList[i] + newDir;
                while ( compVector != cornerVector)
                {

                    tempList.Add(compVector);

                    compVector += newDir;
                    
                }
                
                while(compVector != returnCornerVector)
                {
                   
                    tempList.Add(compVector);

                    compVector += returnDir;
                }
               
                for (int tI = endCutIndex; tI < inList.Count; tI++)
                {
                    tempList.Add(inList[tI]);
                   
                }

                inList.Clear();
                inList = tempList;

                i =  inList.FindIndex(x => x == returnCornerVector);

                i = endCutIndex;

            }

        }

        for (int iP = 0; iP < inList.Count; iP++)
        {
            inList[iP] += offSet;
        }



        return inList;
    }
      

        void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        
      // Stopwatch sw = new Stopwatch();

        //sw.Start();
        Node startNode = grid.getNodeFromWorldPosition(startPos, true);
        Node targetNode = grid.getNodeFromWorldPosition(targetPos, true);

        if(startNode == targetNode)
        {
            
            grid.path.Clear();
            grid.path.Add(targetNode);
            return;
        }

        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);

        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        //bool useD = false;

        while (openSet.Count > 0)
        {

            Node CurrentNode = openSet.RemoveFirst();

            closedSet.Add(CurrentNode);

            if (CurrentNode == targetNode)
            {
                
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbor in grid.GetNeighbours(CurrentNode))//, useD))
            {
                //useD = true;

                if (!neighbor.walkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                
                int newMovementCostToNeighbor = CurrentNode.gCost + GetDistance(CurrentNode, neighbor);

                if( newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = CurrentNode;

                    if (!openSet.Contains(neighbor)){
                        openSet.Add(neighbor);
                        //openSet.UpdateItem(neighbor);
                    }
                }
            }

        }
    }
    
    void RetracePath( Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();

        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        grid.path = path;
    }

    
   int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if(distX > distY)
        {
            return 14 * distY + 10 * (distX - distY);
        }
        
            return 14 * distX + 10 * (distY - distX);

        //return 10 * distX + 10 * distY;

    }
    
}
