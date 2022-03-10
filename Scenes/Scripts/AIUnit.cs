using System.Collections;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    public Transform target;
    public float speed;
    Vector2[] path;
    int targetId;
    Rigidbody2D rb;
    Vector2 dir;
    Vector2 currWaypoint;

    void Start()
    {
        PathManager.RequestPath(transform.position, target.position, OnPathFound);
        rb = GetComponent<Rigidbody2D>();
    }
    void OnPathFound(Vector2[] newpath, bool isSuccessfull)
    {
        if (isSuccessfull)
        {
            path = newpath;
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
    }
    IEnumerator FollowPath()
    {
        currWaypoint = path[0];
        while (true)
        {
            if (Vector2.Distance(transform.position, currWaypoint) < 0.05f)
            {
                targetId++;
                if (targetId >= path.Length)
                {
                    targetId = 0;
                    path = new Vector2[0];
                    dir = Vector2.zero;
                    rb.velocity = Vector2.zero;
                    yield break;
                }
                currWaypoint = path[targetId];
            }
            dir = (currWaypoint - (Vector2)transform.position).normalized * speed * Time.deltaTime;
            rb.velocity = dir;
            yield return null;
        }
    }
    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetId; i < path.Length; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(path[i], Vector3.one * 0.5f);

                if (i == targetId) Gizmos.DrawLine(transform.position, path[i]);
                else Gizmos.DrawLine(path[i - 1], path[i]);
            }
        }
    }
}
