using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//file containing all of the individual states that the AI can use
public class State_Patrol : States
{
    //ended up not being used
    public override void Execute(Agent agent)
    {

        agent.canMove = true;
        agent.SB.PursuitOn();
        
    }
}

public class State_Search_Player : States
{
    //ending up not being used
    public override void Execute(Agent agent)
    {

        agent.canMove = true;
        agent.SB.PursuitOn();

    }
}

public class State_Search_Ammo : States
{
    public bool hasCreatedPath = false; //to stop the path from being created every time the update ticks

    //on execute the state will create a new A* graph that will find the shortest route to the position of the "closestAmmo" object in the world.
    public override void Execute(Agent agent)
    {
        agent.SB.PursuitOff();
        if(hasCreatedPath == false)
        {
            agent.canMove = true;
            agent.SB.PathOff();
            Astarpathfinding pathFind = new Astarpathfinding();
            CreateNodes graphBuilder = new CreateNodes();
            NavGraph tempGraph = graphBuilder.createGraph();
            PathFollow followCode = new PathFollow();
            FindNearestNode nodeFinder = new FindNearestNode();
            pathFind.Initialize(tempGraph);
            pathFind.Check(tempGraph, nodeFinder.FindNearestIndex(agent.transform.position), nodeFinder.FindNearestIndex(agent.closestAmmo.transform.position));
            followCode.CreatePath(pathFind.Path, tempGraph);
            if(followCode.nodePath.Count > 1)
            {
                if (Vector3.Distance(tempGraph.Nodes[followCode.nodePath[0]].position, tempGraph.Nodes[followCode.nodePath[1]].position) > Vector3.Distance(agent.transform.position, tempGraph.Nodes[followCode.nodePath[1]].position))
                {
                    //if the AI is closer to the second node in the path than the first node is it means that the AI has passed the first node, and as such it is no longer needed and can be removed.
                    followCode.nodePath.RemoveAt(0);
                    Debug.Log("new first node is " + followCode.nodePath[0]);
                }
            }
           
            agent.SB.path = followCode;
            agent.SB.PathOn();
            hasCreatedPath = true;
            Debug.Log("searching for ammo");
        }
        //while there are still nodes in the path to the ammo
        if(agent.SB.path.nodePath.Count > 1)
        {
            //if it is within 8 units of the last node in the path the agent will begin to seek towards the actual ammo position instead
            if (Vector3.Distance(agent.transform.position, agent.SB.path.nodeList[agent.SB.path.nodePath[agent.SB.path.nodePath.Count - 1]].position) < 8)
            {
                agent.SB.PathOff();
                agent.SB.SeekOn(agent.closestAmmo.transform.position, 2);
            }
        }
        else
        {
            //if the AI is already next to the ammo when the state is executed it will simply seek to the ammo immediately 
            agent.SB.PathOff();
            agent.SB.SeekOn(agent.closestAmmo.transform.position, 2);
        }
  
     
        //agent.patrolCounter++;
    }
}
public class State_Search_Health : States
{
    public bool hasCreatedHealthPath = false; //to stop the path from being created every time the update ticks
    //works identically to the State_Search_Ammo state
    public override void Execute(Agent agent)
    {
        agent.SB.PursuitOff();
        if (hasCreatedHealthPath == false)
        {
            agent.canMove = true;
          
            agent.SB.PathOff();
            Astarpathfinding pathFind = new Astarpathfinding();
            CreateNodes graphBuilder = new CreateNodes();
            NavGraph tempGraph = graphBuilder.createGraph();
            PathFollow followCode = new PathFollow();
            FindNearestNode nodeFinder = new FindNearestNode();
            pathFind.Initialize(tempGraph);
            pathFind.Check(tempGraph, nodeFinder.FindNearestIndex(agent.transform.position), nodeFinder.FindNearestIndex(agent.closestHp.transform.position));
            followCode.CreatePath(pathFind.Path, tempGraph);
            if (Vector3.Distance(tempGraph.Nodes[followCode.nodePath[0]].position, tempGraph.Nodes[followCode.nodePath[1]].position) > Vector3.Distance(agent.transform.position, tempGraph.Nodes[followCode.nodePath[1]].position) && followCode.nodePath.Count > 1)
            {
                followCode.nodePath.RemoveAt(0);
            }
            agent.SB.path = followCode;
            agent.SB.PathOn();
            hasCreatedHealthPath = true;
        }
        if (Vector3.Distance(agent.transform.position, agent.SB.path.nodeList[agent.SB.path.nodePath[agent.SB.path.nodePath.Count - 1]].position) < 8)
        {
            agent.SB.PathOff();
            agent.SB.SeekOn(agent.closestHp.transform.position, 5);
        }
    }
}
public class State_Flee : States
{
    public override void Execute(Agent agent)
    {
        agent.canMove = true;
        agent.SB.FleeOn(agent.SB.playerChar.transform.position, 100f);
        Debug.Log("fleeing from danger");
    }
}
public class State_Fight_Player : States
{
    Vector3 playerPos = new Vector3(0, 0, 0);
    private float nextTimeToFire = 0f;

    //whether or not the AI can fire is decided by whether the current time is greater than the time during the last instance of the AI firing the gun + the cooldown, which is based on the fire rate of the weapon
    public override void Execute(Agent agent)
    {
        agent.canMove = true;
        agent.SB.PursuitOff();
        if (Time.time >= nextTimeToFire && agent.currentAmmo > 0)
        {
            playerPos = agent.GetComponent<Perception>().MemoryMap[agent.managerState.player].LastSensedPosition;
            nextTimeToFire = Time.time + 1f / agent.fireRate;
            agent.currentAmmo -= 1;
            Debug.Log("Firing");
            Fire(agent);
        }
        Debug.Log("engaging on player");

    }
    private void Fire(Agent agent)
    {
        //having in-depth gunplay mechanics is unnecessary, so a simple linecast to the player will suffice
        if(Physics.Linecast(agent.transform.position, playerPos))
        {
            agent.managerState.player.GetComponent<Player_Gun>().TakeDamage(agent.gunDamage);
        }
    }
}
public class State_Follow_Path : States
{
    //calls the PathOn function in the Agent's steering behaviours 
    public override void Execute(Agent agent)
    {
        agent.canMove = true;
        agent.SB.PathOn();
    }
}
public class State_Chase_Player : States
{
    public override void Execute(Agent agent)
    {
        agent.canMove = true;
        Debug.Log("Chasing the player");
    }
}
public class State_Idle : States
{
    public override void Execute(Agent agent)
    {
        //this state is used for units that just wait in a spot until they have a reason to leave it, does nothing
    }
}
public class State_Return_To_Post : States
{
    //returns the agent to its post location during downtime
    public override void Execute(Agent agent)
    {
        agent.canMove = true;
        agent.SB.SeekOn(agent.managerState.postLocation,5f);

    }       
}