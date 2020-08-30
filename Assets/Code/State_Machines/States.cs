using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class States
{
    //abstact class that is used to create states down the line. Just includes the shared execute function that all states will possess. 


    public abstract void Execute(Agent agent);
  
  
}
