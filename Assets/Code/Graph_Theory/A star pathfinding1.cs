using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astarpathfinding
{
  
    List<int> nodeRoute;
    List<float> nodeCosts;
    List<GraphEdge> traversedEdges;

    List<GraphNode> graphNodes = new List<GraphNode>();
    public List<int> Path;
    bool targetNodeFound = false;
    CustomPriorityQueue<GraphEdge> edgeQueue;
   
    public void Initialize(NavGraph graph)
    {
        int noOfNodes = graph.Nodes.Count;
        nodeRoute = new List<int>();
        nodeRoute.Capacity = noOfNodes;
        nodeCosts = new List<float>();
        nodeCosts.Capacity = noOfNodes;
        traversedEdges = new List<GraphEdge>();
        edgeQueue = new CustomPriorityQueue<GraphEdge>();
        Path = new List<int>();

        //adds elements to the nodeCosts list, starting at the maximum possible amount so that untraversed nodes will always start off with larger values than traversed ones
        for (int i = 0; i < nodeCosts.Capacity; i++)
        {
            float tempadd = float.MaxValue;
            nodeCosts.Add(tempadd);
            //nodeCosts[i] = float.MaxValue;
        }
        //adding default nodes to the route list
        for (int i = 0; i < nodeRoute.Capacity; i++)
        {
            int tempadd = 0;
            nodeRoute.Add(tempadd);
        }
        for(int i = 0; i < traversedEdges.Count; i++)
        {
            traversedEdges[i] = null;
        }
    }

    //finds the distance between two points along right angles
    float manhattanDist(GraphNode toNode, GraphNode fromNode)
    {
        float Distance = Mathf.Abs(toNode.position.x - fromNode.position.x) + Mathf.Abs(toNode.position.y - fromNode.position.y);
        return Distance;
    }


    void GetAdjacentEdges(int sourceNode, List<GraphNode> temp, GraphNode targetNode)
    {
        for (int i = 0; i < temp[sourceNode].adjacencyList.Count; i++)
        {
            float priority = manhattanDist(temp[sourceNode], targetNode); 
            temp[sourceNode].adjacencyList[i].edgeCost = priority;

            edgeQueue.Enqueue(temp[sourceNode].adjacencyList[i]);
           
        }
    }

    public bool Check(NavGraph graph, int sourceNode, int targetNode)
    {
        graphNodes = graph.Nodes;
        nodeCosts[sourceNode] = 0f;
        GetAdjacentEdges(sourceNode, graphNodes, graphNodes[targetNode]);
        
        while (edgeQueue.Count() != 0)
        {
            
            GraphEdge tempEdge = edgeQueue.Dequeue();
            traversedEdges.Add(tempEdge);
            if (nodeCosts[tempEdge.To] > nodeCosts[tempEdge.From] + tempEdge.GetCost())
            {
                nodeRoute[tempEdge.To] = tempEdge.From;
                nodeCosts[tempEdge.To] = nodeCosts[tempEdge.From] + tempEdge.GetCost() + manhattanDist(graphNodes[tempEdge.To],graphNodes[tempEdge.From]);
                
                if (tempEdge.To == targetNode)
                {
                    targetNodeFound = true;
                    continue;
                }
            }
            


            for (int i = 0; i < graphNodes[tempEdge.To].adjacencyList.Count; i++)
            {
                bool canAdd = true; 
                if (traversedEdges.Contains(graphNodes[tempEdge.To].adjacencyList[i]))
                {
                    canAdd = false;
                }
                else if(edgeQueue.data.Contains(graphNodes[tempEdge.To].adjacencyList[i]))
                {
                    canAdd = false;
                }
                if (canAdd == true)
                {
                    float priority = manhattanDist(graphNodes[tempEdge.To], graphNodes[targetNode]); //should set the value to be based off of the manhattan distance
                    graphNodes[tempEdge.To].adjacencyList[i].edgeCost = priority;
                    nodeCosts[tempEdge.To] = nodeCosts[tempEdge.From] + tempEdge.GetCost();
                    edgeQueue.Enqueue(graphNodes[tempEdge.To].adjacencyList[i]);
                }

            }

        }

        if (targetNodeFound == true)
        {
            int currentNode = targetNode;
            Path.Add(currentNode);
            while (currentNode != sourceNode)
            {
                currentNode = nodeRoute[currentNode];
                Path.Add(currentNode);
            }
            return true;

        }
        return false;
    }
}
