using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using System;

public class PathFinding : MonoBehaviour
{
    PathGrid grid;
    PathManager pathManager;
    void Awake()
    {
        grid = GetComponent<PathGrid>();
        pathManager = GetComponent<PathManager>();
    }
    public void RequestPath(Vector2 startPos, Vector2 endPos)
    {
        StartCoroutine(FindPath(startPos,endPos));
    }
    IEnumerator FindPath(Vector2 start, Vector2 end)
    {
        Stopwatch sw = new Stopwatch();
        Node startNode = grid.GetNode(start);
        Node endNode = grid.GetNode(end);
        startNode.gCost = 0;

        Vector2[] wayPoints = new Vector2[0];
        bool isSuccessfull = false;

        Heap<Node> openSet = new Heap<Node>(grid.Area);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            Node currNode = openSet.Take();
            closedSet.Add(currNode);

            if(currNode == endNode)
            {
                Retrace(startNode,endNode);
                isSuccessfull = true;
                break;
            }

            foreach(Node n in grid.GetNeighbors(currNode))
            {
                if(!n.isWalkable || closedSet.Contains(n)) continue;
                
                int newMovementCost = currNode.gCost + CalcDst(currNode, n) + n.penaltyValue;
                if(newMovementCost < n.gCost || !openSet.Contains(n))
                {
                    n.gCost = newMovementCost;
                    n.hCost = CalcDst(n,endNode);
                    n.parent = currNode;

                    if(!openSet.Contains(n)) openSet.Add(n); 
                    else openSet.UpdateItems(n);
                }
            }
        }
        yield return null;
        if(isSuccessfull)
        {
            wayPoints = Retrace(startNode,endNode);
        }
        pathManager.FinishProcess(wayPoints,isSuccessfull);
    }

    Vector2[] Retrace(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node currNode = end;

        while(currNode != start)
        {
            path.Add(currNode);
            currNode = currNode.parent;
        }
        Vector2[] wayPoints = SimplifyPath(path);
        Array.Reverse(wayPoints);
        return wayPoints;
    }
    Vector2[] SimplifyPath(List<Node> path)
    {
        List<Vector2> wayPoints = new List<Vector2>();
        Vector2 oldDir = Vector2.zero;
        for(int i = 1; i < path.Count; i++)
        {
            Vector2 newDir = new Vector2(path[i - 1].posX - path[i].posX, path[i-1].posY - path[i].posY);
            if(oldDir != newDir) wayPoints.Add(path[i].worldPoint);
            oldDir = newDir;
        }
        return wayPoints.ToArray();
    }
    int CalcDst(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.posX - b.posX);
        int dstY = Mathf.Abs(a.posY - b.posY);

        if(dstX > dstY) return 14*dstY + 10*(dstX - dstY);
        else return 14*dstX + 10*(dstY - dstX);
    }
}
