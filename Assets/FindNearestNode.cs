using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindNearestNode 
{
   public int FindNearestIndex(Vector3 position)
    {
        CreateNodes nodeMaker = new CreateNodes();
        int nearestNode = 0; //the indext of the current nearest node
        float closestDistance = float.MaxValue; //the distance to the current closest node
        NavGraph graph = nodeMaker.createGraph();
        for(int i = 0; i < graph.Nodes.Count; i++)
        {
            float tempDistance = Vector3.Distance(position, graph.Nodes[i].position);
            if(tempDistance < closestDistance)
            {
                closestDistance = tempDistance;
                nearestNode = graph.Nodes[i].index;
            }
        }
        Debug.Log("nearest index is " + nearestNode);
        return nearestNode;
    }
}
