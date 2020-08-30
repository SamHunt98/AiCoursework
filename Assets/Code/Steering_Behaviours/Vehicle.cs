using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteeringBehaviours))]
public class Vehicle : MonoBehaviour {

    public Vector3 Velocity;
    //Represents the weight of an object, will effect its acceleration
    public float Mass = 1;

    //The maximum speed this agent can move per second
    public float MaxSpeed = 18; //running

    public float Speed; //changes to either of the two values

    public float casualSpeed = 12; //when no threat is detected

    //The thrust this agent can produce
    public float MaxForce = 1;

    //We use this to determine how fast the agent can turn
    public float MaxTurnRate = 1.0f;

    public GameObject whiskerRight; //object that will be used as the starting position for a raycast that will spot walls to the side of the player, similar to a cat's whisker 
    public GameObject whiskerLeft;

    public SteeringBehaviours SB; //used to gain access to the SteeringBehaviours functions

    public Agent agent;
    public PathFollow path;

    // Use this for initialization
    void Start ()
    {
        agent = GetComponent<Agent>();
        SB = GetComponent<SteeringBehaviours>();
        Speed = casualSpeed;
	}
	
	void Update ()
    {
        Vector3 SteeringForce;


        if (agent.canMove)
        {
            if (SB.obstacleList.Count == 0)
            {
                SteeringForce = SB.Calculate();
            }
            else
            {
                SteeringForce = SB.Calculate();
                SteeringForce += SB.getObstacleAvoidance();
            }
            if (SteeringForce != Vector3.zero)
            {
                SteeringForce += SB.GetWall(this.gameObject, 30);
                SteeringForce += SB.GetWall(whiskerLeft, 15);
                SteeringForce += SB.GetWall(whiskerRight, 15);

                SteeringForce.y = 0;
                Vector3 Acceleration = SteeringForce / Mass;

                Velocity += Acceleration * Time.deltaTime;

                Velocity = Vector3.ClampMagnitude(Velocity, Speed);

                if (Velocity != Vector3.zero)
                {
                    transform.position += Velocity * Time.deltaTime;

                    transform.forward = Velocity.normalized;
                }
            }
        }
        else
        {
            Velocity = Vector3.zero; 
        }
	}
}
