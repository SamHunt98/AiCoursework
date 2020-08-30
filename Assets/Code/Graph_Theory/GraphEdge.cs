using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphEdge : IComparable<GraphEdge>
{
    public int From; //stores the index of the first node on the edge
    public int To; //stores the index value of the second node on the edge
    public float edgeCost; //stores the cost of traversing this edge
 


    public GraphEdge(int from, int to, float cost)
    {
        this.From = from;
        this.To = to;
        this.edgeCost = cost;
    }
    public float GetCost()
    {
        return edgeCost;
    }

    public int CompareTo(GraphEdge other)
    {
        if (this.edgeCost < other.edgeCost) return -1;
        else if (this.edgeCost > other.edgeCost) return 1;
        else return 0;
    }
}
