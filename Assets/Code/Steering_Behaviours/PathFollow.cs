using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollow
{
    // Start is called before the first frame update
    public List<int> nodePath = new List<int>();
    public List<GraphNode> nodeList = new List<GraphNode>();

    public int CurrentWaypoint; //stores the current value of the list that the actor is travelling to

    public void CreatePath(List<int> path, NavGraph graph)
    {
        nodeList = graph.Nodes;
        Debug.Log("Size of the path is " + path.Count);
        for (int i = 0; i < path.Count; i++)
        {
            //due to the way that the A* function adds elements to the list, the node path will need to add elements in reverse
            nodePath.Add(path[path.Count - (1 + i)]);
            Debug.Log("Path value added: " + nodePath[i]);
        }
    }

    public void SetNextWaypoint()
    {
        CurrentWaypoint += 1;
    }
}
