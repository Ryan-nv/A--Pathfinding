using System.Collections.Generic;
using UnityEngine;

public class PathGrid : MonoBehaviour
{
    public LayerMask obstacleMask;
    public int obstacleProximityPenalty = 20;
    public int weightStrength;
    public Vector2 gridArea;
    public float gridSize;
    //smart path attributes
    public TerrainType[] terrainTypes;
    LayerMask walkableMask;
    //testing
    public bool drawGizmos;
    int maxPenalty;
    int minPenalty;
    Dictionary<int, int> walkableTerrain = new Dictionary<int, int>();

    Node[,] grid;
    float gridRadius;
    int gridCountX, gridCountY;

    public int Area
    {
        get { return gridCountX * gridCountY; }
    }
    void Awake()
    {
        gridRadius = gridSize * 0.5f;
        gridCountX = Mathf.RoundToInt(gridArea.x / gridSize);
        gridCountY = Mathf.RoundToInt(gridArea.y / gridSize);
        foreach (TerrainType terrain in terrainTypes)
        {
            walkableMask.value |= terrain.layerMask.value;
            walkableTerrain.Add((int)Mathf.Log(terrain.layerMask.value, 2), terrain.penalty);
        }
        CreateGrid();
    }
    void CreateGrid()
    {
        grid = new Node[gridCountX, gridCountY];
        //Find the bottom left corner of the total gridsize
        Vector2 bottomLeft = (Vector2)transform.position - Vector2.right * gridCountX * gridSize * .5f - Vector2.up * gridCountY * gridSize * .5f;
        for (int x = 0; x < gridCountX; x++)
        {
            for (int y = 0; y < gridCountY; y++)
            {
                Vector2 worldPoint = bottomLeft + Vector2.right * (x * gridSize + gridRadius) + Vector2.up * (y * gridSize + gridRadius);
                bool isWalkable = !(Physics2D.OverlapCircle(worldPoint, gridRadius, obstacleMask));
                //assign the penalty value
                int movementPenalty = 0;
                Collider2D obj = Physics2D.OverlapCircle(worldPoint, gridRadius * 0.5f, walkableMask);
                if (obj)
                {
                    walkableTerrain.TryGetValue(obj.gameObject.layer, out movementPenalty);
                }
                if (!isWalkable)
                {
                    movementPenalty += obstacleProximityPenalty;
                }
                grid[x, y] = new Node(isWalkable, worldPoint, x, y, movementPenalty);
            }
        }
        BlurPenalty(weightStrength);
    }
    void BlurPenalty(int num)
    {
        int kernelSize = num * 2 + 1;
        int kernelExtent = (int)((kernelSize - 1) * 0.5);

        int[,] horizontalPass = new int[gridCountX, gridCountY];
        int[,] verticalPass = new int[gridCountX, gridCountY];
        //horizontal calculation
        for (int y = 0; y < gridCountY; y++)
        {
            for (int x = -kernelExtent; x < kernelExtent; x++)
            {
                int idX = Mathf.Clamp(x, 0, kernelExtent);
                horizontalPass[0, y] += grid[idX, y].penaltyValue;
            }
            for (int x = 1; x < gridCountX; x++)
            {
                int prevId = Mathf.Clamp(x - kernelExtent - 1, 0, gridCountX);
                int nextId = Mathf.Clamp(x + kernelExtent, 0, gridCountX - 1);
                horizontalPass[x, y] = horizontalPass[x - 1, y] - grid[prevId, y].penaltyValue + grid[nextId, y].penaltyValue;
            }
        }
        //vertical calculation
        for (int x = 0; x < gridCountX; x++)
        {
            for (int y = -kernelExtent; y < kernelExtent; y++)
            {
                int idY = Mathf.Clamp(y, 0, kernelExtent);
                verticalPass[x, 0] += horizontalPass[x, idY];
            }

            int blurPenalty = Mathf.RoundToInt((float)verticalPass[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].penaltyValue = blurPenalty;

            for (int y = 1; y < gridCountY; y++)
            {
                int prevId = Mathf.Clamp(y - kernelExtent - 1, 0, gridCountY);
                int nextId = Mathf.Clamp(y + kernelExtent, 0, gridCountY - 1);
                verticalPass[x, y] = horizontalPass[x, y - 1] - horizontalPass[x, prevId] + horizontalPass[x, nextId];
                //apply the blurred penalty 
                blurPenalty = Mathf.RoundToInt((float)verticalPass[x, y] / (kernelSize * kernelSize));
                grid[x, y].penaltyValue = blurPenalty;

                if(blurPenalty < minPenalty) minPenalty = blurPenalty;
                if(blurPenalty > maxPenalty) maxPenalty = blurPenalty;
            }
        }
    }
    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                int coordX = node.posX + x;
                int coordY = node.posY + y;
                if ((coordX >= 0 && coordX < gridCountX) && (coordY >= 0 && coordY < gridCountY)) neighbors.Add(grid[coordX, coordY]);
            }
        }
        return neighbors;
    }
    public Node GetNode(Vector2 worldPos)
    {
        float percentX = worldPos.x / gridArea.x + .5f;
        float percentY = worldPos.y / gridArea.y + .5f;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        int x = Mathf.FloorToInt(Mathf.Min(gridCountX * percentX, gridCountX - 1));
        int y = Mathf.FloorToInt(Mathf.Min(gridCountY * percentY, gridCountY - 1));
        return grid[x, y];
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, gridArea);
        if(grid != null && drawGizmos)
        {
            foreach(Node n in grid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(minPenalty, maxPenalty, n.penaltyValue));
                Gizmos.color = (n.isWalkable) ? Gizmos.color : Color.red;
                Gizmos.DrawCube(n.worldPoint,gridSize * Vector3.one);
            }
        }
    }
    [System.Serializable]
    public class TerrainType
    {
        public LayerMask layerMask;
        public int penalty;
    }
}
