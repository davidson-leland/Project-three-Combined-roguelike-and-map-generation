using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

    Node[,] grid;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public LayerMask unwalkableMask;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    public Transform player, target;

    public List<Node> path;
    public bool useSimple = false;

    void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        //CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

   public void CreateGrid(Room inRoom)
    {

        useSimple = true;
        grid = new Node[inRoom.rows, inRoom.columns];

        gridSizeX = inRoom.rows;
        gridSizeY = inRoom.columns;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = new Vector3(x, y, 0) + inRoom.mapOffSet;

                bool walkable = inRoom.map[x, y] == 0;

                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }

        gridWorldSize.x = gridSizeX;
        gridWorldSize.y = gridSizeY;

    }

    public Node getNodeFromWorldPosition(Vector3 worldPosition, bool doLog = false)
    {
        if (useSimple)
        {

            if(doLog)
            {
                //Debug.Log("worldposition.x = " + worldPosition.x + ", between " + grid[0, 0].gridX + " and " + grid[gridSizeX - 1, gridSizeY - 1].gridX);
                

               // Debug.Log("worldposition.y = " + worldPosition.y + ", between " + grid[0, 0].gridY + " and " + grid[gridSizeX - 1, gridSizeY - 1].gridY);
                //Debug.Log("between " + grid[0, 0].gridY + " and " + grid[gridSizeX - 1, gridSizeY - 1].gridY);
            }
            return grid[(int)worldPosition.x, (int)worldPosition.y];
        }

        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public bool ConTainsCheck(Vector3 inVector)
    {
        
        if ( inVector.x >= 0 && inVector.x <= gridSizeX - 1)
        {
            if(inVector.y >= 0 && inVector.y <= gridSizeY - 1)
            {
                return true;
            }
        }

        return false;
    }

    public List<Node> GetNeighbours(Node node, bool useD = true)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
               if(x== 0 && y == 0  )//|| !useD && Mathf.Abs(x) + Mathf.Abs(y) == 2)//2nd portion ignores diagonals
                {
                    continue;
                }
                    
                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        neighbors.Add(grid[checkX, checkY]);
                    }  
            }
        }

        return neighbors;
    }

    /*

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if(grid != null)
        {
            Node playerNode = getNodeFromWorldPosition(player.position);
            Node targetNode = getNodeFromWorldPosition(target.position);


            
            if(path != null)
            {
                foreach (Node n in path)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
            }
            
            
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                if(playerNode == n || targetNode == n)
                {
                    Gizmos.color = Color.cyan;
                }

                if(path != null)
                {
                    if (path.Contains(n))
                    {
                        Gizmos.color = Color.black;
                        //Debug.Log(" path Node" + n.gridX + ", " + n.gridY);
                        //Debug.Log("parent = " + n.parent.gridX + " " + n.parent.gridY);
                    }
                }
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));

            }
        }
    }
*/

}
