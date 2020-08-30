using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Vehicle))]
public class SteeringBehaviours : MonoBehaviour {

    Vehicle vehicle;
    Agent agent;
    public GameObject playerChar;
    //SeekOn
    public bool IsSeekOn = false;
    public Vector3 SeekOnTargetPos;
    float SeekOnStopDistance = 5;

    //pathfind on
    public bool isPathOn = false;
    public PathFollow path;
    //fleeOn
    bool IsFleeOn = false;
    Vector3 FleeOnTargetPos;
    float FleeOnStopDistance;
    //WanderOn
    bool IsWanderOn = false;
    public float WanderRadius = 10f;
    public float WanderDistance = 10f;
    public float WanderJitter = 1f;
    Vector3 WanderTarget = Vector3.zero;

    //Arrive variables 
    float slowingDistance = 10.0f;
    float distThreshold = 15.0f;
    Vector3 dVelocity = new Vector3(0, 0, 0);

    //pursuit on
    bool IsPursuitOn;
    Vector3 pursuitResult; //stores the result of the seek value from the pursuit function so it can be used in calculate
    BoxCollider DetectionBox; //possible object avoidance stuff
    GameObject[] detectedObjects;

    //collision detection 
    public float maxSeeAhead = 5; //maximum distance the object can see ahead of it
    public GameObject detectBoxHolder;
    public BoxCollider detectBox;
    public GameObject detectBoxScaler;
    public List<Collider> obstacleList = new List<Collider>(); //holds the data for all the colliders within the detection box
    public GameObject closestObstacle;
    public float closestDistance = -50000; //set to -50000 as a default that will be overwritten later

    //wall avoidance 
    public GameObject rightWhisker;
    public GameObject leftWhisker;
    public LayerMask wallMask;

	void Start ()
    {
        detectBox = detectBoxHolder.GetComponent<BoxCollider>();
        
        vehicle = GetComponent<Vehicle>();
        agent = GetComponent<Agent>();
        //Set an initial wander target
        WanderTarget = new Vector3(Random.Range(-WanderRadius, WanderRadius), 0, Random.Range(-WanderRadius, WanderRadius));
	}
	
	void Update ()
    {
        getCollisionDetections();
        if(obstacleList.Count !=0)
        {
            GetClosestObstacle();
        }
        else
        {
            closestDistance = 50000;
        }
	}

    public Vector3 Calculate()
    {
        Vector3 VelocitySum = Vector3.zero;

        if (IsSeekOn)
        {
            if (Vector3.Distance(transform.position, SeekOnTargetPos) <= SeekOnStopDistance)
            {
                //We're close enough to "stop"
                IsSeekOn = false;

                //Set the vehicle's velocity back to zero
                vehicle.Velocity = Vector3.zero;
            }
            else
            {
                VelocitySum += Seek(SeekOnTargetPos);
            }
        }
        if(isPathOn)
        {
            VelocitySum += pathfollow(path);
        }
        if(IsFleeOn)
        {
            if (Vector3.Distance(transform.position, FleeOnTargetPos) >= FleeOnStopDistance)
            {
                IsFleeOn = false;
                vehicle.Velocity = Vector3.zero;
            }
            else
            {
                VelocitySum += Flee(FleeOnTargetPos);
            }
        }

        if (IsWanderOn)
        {
            VelocitySum += Wander();
        }
        if(IsPursuitOn)
        {
            Pursuit(playerChar);
            VelocitySum += pursuitResult;
        }

        return VelocitySum;
    }

    Vector3 Seek(Vector3 TargetPos)
    {
        Vector3 DesiredVelocity = (TargetPos - transform.position).normalized * vehicle.Speed;
        return (DesiredVelocity - vehicle.Velocity);
    }

    Vector3 Flee(Vector3 TargetPos)
    {
     
        Vector3 DesiredVelocity = (transform.position - TargetPos).normalized * vehicle.MaxSpeed;

        return (DesiredVelocity - vehicle.Velocity);

        
    }

    Vector3 Wander()
    {
        WanderTarget += new Vector3(
            Random.Range(-1f, 1f) * WanderJitter,
            0,
            Random.Range(-1f, 1f) * WanderJitter);

        WanderTarget.Normalize();

        WanderTarget *= WanderRadius;

        Vector3 targetLocal = WanderTarget;

        Vector3 targetWorld = transform.position + WanderTarget;

        targetWorld += transform.forward * WanderDistance;

        return targetWorld - transform.position;
    }
    public Vector3 Arrive(Vector3 TargetPos)
    {

        Vector3 ToTarget = (TargetPos - transform.position);

        float Distance = ToTarget.magnitude;

        if (Distance <= distThreshold)
        {
            Debug.Log(Distance);
            float Speed = Distance / slowingDistance;
            Mathf.Clamp(Speed, 0.0f, vehicle.MaxSpeed);
            Debug.Log(Speed);
            dVelocity = (ToTarget.normalized * Speed / Distance);
            Debug.Log("dvelocity is " + dVelocity);
        }
        else
        {
            return Seek(TargetPos);
        }
        return (dVelocity - vehicle.Velocity);

    }
    float CalculateBoxLength()
    {
        //used to calculate the length of the box that is projected in front of the AI
        //the box grows larger as the AI gains speed so that it has enough time to react and turn 
        float ahead = maxSeeAhead * vehicle.Speed;
        return ahead;
    }

    public void Pursuit(GameObject Evader) 
    {
        PlayerMove charMovement = Evader.GetComponent<PlayerMove>();
        Vector3 ToEvader = Evader.transform.position - transform.position;
        float RelativeHeading = Vector3.Dot(transform.forward.normalized, Evader.transform.forward.normalized);

        if (RelativeHeading >= 0)
        {
            pursuitResult = Seek(Evader.transform.position);
        }
        else
        {
            float LookAheadTime = ToEvader.magnitude / (vehicle.MaxSpeed + charMovement.currentSpeed); 
            Vector3 EvaderFuturePosition = Evader.transform.position + (Evader.transform.forward * LookAheadTime); 
            pursuitResult = Seek(EvaderFuturePosition);
        }

    }

    public void Evade(Vehicle Pursuer)
    {
        Vector3 ToPursuer = Pursuer.transform.position - transform.position;
        float LookAheadTime = ToPursuer.magnitude / (vehicle.MaxSpeed + Pursuer.MaxSpeed);
        Vector3 PursuerFuturePosition = Pursuer.transform.position + Pursuer.Velocity * LookAheadTime;
        Flee(PursuerFuturePosition);
    }
    /// <summary>
    /// Will Seek to TargetPos until within StopDistance range from it
    /// </summary>
    /// <param name="TargetPos"></param>
    /// <param name="StopDistance"></param>
    public void SeekOn(Vector3 TargetPos, float StopDistance = 0.01f)
    {
        IsSeekOn = true;
        SeekOnTargetPos = TargetPos;
        SeekOnStopDistance = StopDistance;
    }
    public void PursuitOn()
    {
        IsSeekOn = false;
        IsFleeOn = false;
        IsPursuitOn = true;
    }
    public void PursuitOff()
    {
        IsPursuitOn = false;
    }
    public void FleeOn(Vector3 TargetPos, float StopDistance = 0.01f)
    {
        IsFleeOn = true;
        IsSeekOn = false;
        IsPursuitOn = false;
        FleeOnTargetPos = TargetPos;
        FleeOnStopDistance = StopDistance;
    }

    public void WanderOn()
    {
        IsWanderOn = true;
    }

    public void WanderOff()
    {
        IsWanderOn = false;
        vehicle.Velocity = Vector3.zero;
    }
    public void getCollisionDetections()
    {
        detectBoxScaler.transform.localScale = new Vector3(1,1,vehicle.Velocity.magnitude);
    }
    public Vector3 getObstacleAvoidance()
    {
        Vector3 steeringForce = new Vector3(0,0,0);
        Transform vehicleLoc = this.transform;
        Vector3 obsRelative = vehicleLoc.InverseTransformPoint(closestObstacle.transform.position);
        float forceMultiplier = 1.0f + (detectBoxScaler.transform.localScale.z - closestObstacle.transform.localPosition.x) / detectBoxScaler.transform.localScale.z;
        steeringForce.x = (closestObstacle.GetComponent<SphereCollider>().radius - closestObstacle.transform.localPosition.z) * forceMultiplier;
        Vector3 worldVector = transform.TransformDirection(steeringForce);
        return worldVector;
    }
    

    public void GetClosestObstacle()
    {
        
        float tempdistance;

        for (int i = 0; i < obstacleList.Count; i++)
        {
            if(closestDistance != -50000)
            {
                tempdistance = Vector3.Distance(transform.position, obstacleList[i].gameObject.transform.position);
                if(tempdistance < closestDistance)
                {
                    closestDistance = tempdistance;
                    closestObstacle =  obstacleList[i].gameObject;
                }
            }
            else
            {
                closestDistance = Vector3.Distance(transform.position, obstacleList[i].gameObject.transform.position);
                closestObstacle = obstacleList[i].gameObject;
            }
        }
    }
    public Vector3 GetWall(GameObject whisker, float rayLength)
    {
        RaycastHit hit;
        Vector3 steeringForce = new Vector3(0,0,0);
        if(Physics.Raycast(whisker.transform.position, whisker.transform.TransformDirection(Vector3.forward),out hit, rayLength, wallMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            float overlap = rayLength - hit.distance;
            Vector3 wallNormal = hit.normal;
            steeringForce = wallNormal * overlap;
            steeringForce.y = 0;
        }
        return steeringForce;
    }

    public Vector3 pathfollow(PathFollow pathToFollow)
    {
        if(Vector3.Distance(agent.transform.position, pathToFollow.nodeList[pathToFollow.nodePath[pathToFollow.CurrentWaypoint]].position) < 10)
        {
            if (pathToFollow.CurrentWaypoint  +1 == pathToFollow.nodePath.Count)
            {

            }
            else
            {
                pathToFollow.SetNextWaypoint();
            }
           
          
        }
        if (pathToFollow.CurrentWaypoint == pathToFollow.nodePath.Count -1)
        {
            return Seek(pathToFollow.nodeList[pathToFollow.nodePath[pathToFollow.CurrentWaypoint]].position);
        }
        else
        {
            return Seek(pathToFollow.nodeList[pathToFollow.nodePath[pathToFollow.CurrentWaypoint]].position);
        }
    }
    public void PathOn()
    {
        isPathOn = true;
        IsSeekOn = false;
        IsFleeOn = false;
        IsPursuitOn = false;
    }
    public void PathOff()
    {
        isPathOn = false;
    }
    private void OnTriggerEnter(Collider col)
    {
        //if the object isn't already in the list, and is deemed to be an obstacle, add it to the list of obstacles
        if (!obstacleList.Contains(col))
        {
            if(col.gameObject.tag == "AI_obstacle")
            {
                obstacleList.Add(col);
                Debug.Log(col.gameObject.name);
            }
            
            
            
        }
    }
    private void OnTriggerExit(Collider col)
    {
        //if the object is in the list
        if (obstacleList.Contains(col))
        {
            //remove it from the list
            obstacleList.Remove(col);
            Debug.Log("removed" + col.gameObject.name);
        }
    }

    }
