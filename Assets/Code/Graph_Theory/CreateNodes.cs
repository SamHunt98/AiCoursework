using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNodes 
{
    public NavGraph GraphList;

   
    public List<GraphNode> temp;

  
    //this script is used to create the node objects that will be used in the A* pathfinding. 
    //due to the small scope of the project this is being done manually with hardcoded values for each node. In a larger project separate files would potentially be read in depending on the level.

    GameObject node1 = GameObject.Find("Node 0");
    GameObject node2 = GameObject.Find("Node 1");
    GameObject node3 = GameObject.Find("Node 2");
    GameObject node4 = GameObject.Find("Node 3");
    GameObject node5 = GameObject.Find("Node 4");
    GameObject node6 = GameObject.Find("Node 5");
    GameObject node7 = GameObject.Find("Node 6");
    GameObject node8 = GameObject.Find("Node 7");
    GameObject node9 = GameObject.Find("Node 8");
    
    public NavGraph createGraph()
    {
        temp = new List<GraphNode>();


        GraphNode tempNode = new GraphNode();

        tempNode.index = 0; //start

        tempNode.adjacencyList = new List<GraphEdge>();

        //adds edges to the adjacency list
        tempNode.adjacencyList.Add(new GraphEdge(0, 1, 1));
        tempNode.adjacencyList.Add(new GraphEdge(0, 7, 1));
        //fills in the position variable so that the AI can path to the location of the node
        tempNode.position = node1.transform.position;

        temp.Add(tempNode);

        tempNode = new GraphNode();

        tempNode.index = 1;

        tempNode.adjacencyList = new List<GraphEdge>();
        tempNode.adjacencyList.Add(new GraphEdge(1, 2, 1));
        tempNode.adjacencyList.Add(new GraphEdge(1, 0, 1));
        Debug.Log("graph edge thingy is " + tempNode.adjacencyList[0].edgeCost);
        tempNode.position = node2.transform.position;
        temp.Add(tempNode);

        tempNode = new GraphNode();

        tempNode.index = 2;

        tempNode.adjacencyList = new List<GraphEdge>();
        tempNode.adjacencyList.Add(new GraphEdge(2, 3, 1));
        tempNode.adjacencyList.Add(new GraphEdge(2, 1, 1));
        tempNode.position = node3.transform.position;
        temp.Add(tempNode);


        tempNode = new GraphNode();

        tempNode.index = 3;

        tempNode.adjacencyList = new List<GraphEdge>();
        tempNode.adjacencyList.Add(new GraphEdge(3, 4, 1));
        tempNode.adjacencyList.Add(new GraphEdge(3, 2, 1));
        tempNode.position = node4.transform.position;
        temp.Add(tempNode);

        tempNode = new GraphNode();

        tempNode.index = 4;

        tempNode.adjacencyList = new List<GraphEdge>();
        tempNode.adjacencyList.Add(new GraphEdge(4, 5, 1));
        tempNode.adjacencyList.Add(new GraphEdge(4, 3, 1));
        tempNode.adjacencyList.Add(new GraphEdge(4, 8, 1));
        tempNode.position = node5.transform.position;
        temp.Add(tempNode);

        tempNode = new GraphNode();
        tempNode.index = 5;

        tempNode.adjacencyList = new List<GraphEdge>();
        tempNode.adjacencyList.Add(new GraphEdge(5, 6, 1));
        tempNode.adjacencyList.Add(new GraphEdge(5, 4, 1));
        tempNode.position = node6.transform.position;
        temp.Add(tempNode);

        tempNode = new GraphNode();
        tempNode.index = 6;

        tempNode.adjacencyList = new List<GraphEdge>();
        tempNode.adjacencyList.Add(new GraphEdge(6, 7, 1));
        tempNode.adjacencyList.Add(new GraphEdge(6, 5, 1));
        tempNode.position = node7.transform.position;
        temp.Add(tempNode);

        tempNode = new GraphNode();
        tempNode.index = 7;

        tempNode.adjacencyList = new List<GraphEdge>();
        tempNode.adjacencyList.Add(new GraphEdge(7, 0, 1));
        tempNode.adjacencyList.Add(new GraphEdge(7, 6, 1));
        tempNode.position = node8.transform.position;
        temp.Add(tempNode);

        tempNode = new GraphNode();
        tempNode.index = 8;

        tempNode.adjacencyList = new List<GraphEdge>();
        tempNode.adjacencyList.Add(new GraphEdge(8, 4, 1));
        tempNode.position = node9.transform.position;

        temp.Add(tempNode);

        GraphList = new NavGraph();

        for (int i = 0; i < temp.Count; i++)
        {
            GraphList.Nodes.Add(temp[i]);
        }
        return GraphList;
    }

 
}
