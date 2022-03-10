using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector2 worldPoint;
    public bool isWalkable;
    public int posX;
    public int posY;
    public Node parent;
    public int penaltyValue;
    
    private int heapIndex;
    public int hCost;//distance from the endNode
    public int gCost;//distance from the startnode
    public int fCost
    {
        get{return gCost + hCost;}
    }

    public Node(bool _isWalkable, Vector3 _worldPoint, int x, int y, int penalty)
    {
        worldPoint = _worldPoint;
        isWalkable = _isWalkable; 
        penaltyValue = penalty;
        posX = x;
        posY = y;
    }

    public int CompareTo(Node node)
    {
        int comparison = fCost.CompareTo(node.fCost);
        if(comparison == 0)
        {
            comparison = hCost.CompareTo(node.hCost);
        }
        return -comparison;
    }
    public int HeapIndex 
    {
        get {return heapIndex;}
        set{heapIndex = value;}
    }
}
