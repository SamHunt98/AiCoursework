using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode 
{

    public int index = 0; //every node has an index. a valid index is >= 0
    public Vector3 position = new Vector3(0,0,0); //will be equal to the game object it is attached to's location, used to calculate manhattan distance

    public List<GraphEdge> adjacencyList = new List<GraphEdge>();


}
