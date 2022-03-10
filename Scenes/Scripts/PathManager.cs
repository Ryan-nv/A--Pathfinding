using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathManager : MonoBehaviour
{
    Queue<PathRequest> requests = new Queue<PathRequest>();
    PathRequest currPathRequest;
    static PathManager instance;
    PathFinding pathFinding;
    bool isProcessingRequest;
    void Awake()
    {
        instance = this;
        pathFinding = GetComponent<PathFinding>();
    }
    public static void RequestPath(Vector2 pathStart, Vector2 pathEnd, Action<Vector2[], bool> callback)
    {
        PathRequest newPathRequest = new PathRequest(pathStart, pathEnd, callback);
        instance.requests.Enqueue(newPathRequest);
        instance.TryExecuteNext();
    }

    void TryExecuteNext()
    {
        if (!isProcessingRequest && requests.Count > 0)
        {
            currPathRequest = requests.Dequeue();
            isProcessingRequest = true;
            pathFinding.RequestPath(currPathRequest.pathStart,currPathRequest.pathEnd);
        }
    }
    public void FinishProcess(Vector2[] path, bool success)
    {
        currPathRequest.callback(path,success);
        isProcessingRequest = false;
        TryExecuteNext();
    }

    struct PathRequest
    {
        public Vector2 pathStart;
        public Vector2 pathEnd;
        public Action<Vector2[], bool> callback;

        public PathRequest(Vector2 start, Vector2 end, Action<Vector2[], bool> act)
        {
            pathStart = start;
            pathEnd = end;
            callback = act;
        }
    }
}
