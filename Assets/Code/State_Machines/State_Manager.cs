using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class State_Manager : MonoBehaviour
{
    States nextState;
    Agent agent;
    //desirability variables - will go from 0 to 100
    public float hpDesire;
    public float hpStatus;
    public float weaponDesire;
    public float powerful; //how powerful the unit feels, signifying if they will search for the player
    public float attackDesire; //how much they want to attack the player at each moment
    public float wanderDesire; //if a unit is set to be a roaming unit their default desire will be to roam around the map
    public float postDesire; //if a unit is set to have a post to guard their default desire will be to stand in that position
    public int k =1; //used as a multiplier for the function - not hugely relevant

    //personality modifiers
    public int Personality; //0 = coward, 1 = regular, 2 = aggressive
    public float fightModifier; //multiplies the desirability of aggressive actions such as chasing the player or attacking
    public float runModifier; //multiplies the desirability of passive actions such as running away or going to heal
    public bool isRoaming; //dictates whether a unit will want to roam the map for their default behaviour or stand at a guard post

    bool HP_State_On = false; //checks to see if it is in the state it is trying to transition to already to avoid constantly creating a new version of the state
    bool Flee_State_On = false;
    bool Ammo_State_On = false;
    bool Search_State_On = false; 
    bool Fight_State_On = false;
    bool Chase_State_On = false;
    bool Post_State_On = false;
    TimeSpan timeSinceSeen = new TimeSpan(); //the difference in time between now and when the unit last saw the player
    float timeToForget = 50f; //the amount of seconds it takes for the unit to forget about the player once seen.

    public Vector3 postLocation = new Vector3(0,0,0); //stores the location of the post that the guard should stand at 

    public List<GameObject> nodeLocations;
    public bool tempGoToPlace = true; //just to test path finding
    public NavGraph tempGraph = new NavGraph();
    public GraphNode tempNode = new GraphNode();
    public List<int> pathToFollow = new List<int>();
    public Vehicle vehicle;

    public GameObject player; //stores the value for the player character
    List<GameObject> allies = new List<GameObject>();
    public float deadAllyCounter; //will rise each time a new dead ally is found
    List<bool> hasBeenFoundDead = new List<bool>();
    void Start()
    {     
        hpDesire = 0;
        hpStatus = 0;
        weaponDesire = 0;
        powerful = 0;
        agent = GetComponentInParent<Agent>();
        vehicle = GetComponentInParent<Vehicle>();
        GameObject[] tempallies = GameObject.FindGameObjectsWithTag("Enemy_AI"); 
        
        for(int i = 0; i < tempallies.Length; i++)
        {
            allies.Add(tempallies[i]);
         
        }
        for (int i = 0; i < allies.Count; i++)
        {
        
            if (allies[i] == agent.gameObject)
            {
                allies.Remove(allies[i]);
            }
        }
        if (allies.Count > 0)
        {
            
            hasBeenFoundDead.Capacity = allies.Count;
            Debug.Log(allies.Count);
            for(int i = 0; i < hasBeenFoundDead.Capacity; i++)
            {
                hasBeenFoundDead.Add(false);
            }
        }
        if (isRoaming)
        {
            wanderDesire = 0.1f;
            postDesire = 0f;
            State_Patrol patrolState = new State_Patrol();
            agent.ChangeState(patrolState);
        }
        else
        {
            wanderDesire = 0f;
            postDesire = 0.1f;
            Debug.Log("is it here?");
            State_Idle idleState = new State_Idle();
            agent.ChangeState(idleState);
        }
        switch (Personality)
        {
            case 0: //the cowardly unit
                fightModifier = 0.7f;
                runModifier = 1.3f;
                break;
            case 1: //the regular unit that does not have edited values
                fightModifier = 1f;
                runModifier = 1f;
                break;
            case 2:
                fightModifier = 1.5f;
                runModifier = 0.7f;
                break;
        }   
        
    }


    void Update()
    {
        if (agent.gameObject.GetComponent<Perception>().MemoryMap.ContainsKey(player))
        {
            timeSinceSeen = DateTime.Now - agent.gameObject.GetComponent<Perception>().MemoryMap[player].TimeLastSensed;

        }
        else
        {
            powerful = 0; //if the player has never been seen then the power is set to 0 by default to stop it from wanting to fight something it doesn't know exists
        }
        for(int i = 0; i < allies.Count; i++)
        {
            if (hasBeenFoundDead[i] == false)
            {
                if (agent.gameObject.GetComponent<Perception>().MemoryMap.ContainsKey(allies[i]))
                {
                    if (agent.gameObject.GetComponent<Perception>().MemoryMap[allies[i]].targetDead == true)
                    {
                        deadAllyCounter += 0.1f;
                        hasBeenFoundDead[i] = true;
                    }
                }
            }
        
        }
        
        List<float> desireList = new List<float>();
        desireList.Add(weaponDesire);
        desireList.Add(hpDesire);
        desireList.Add(powerful);
        desireList.Add(wanderDesire);
        desireList.Add(postDesire);
        
        if(timeSinceSeen.Seconds > timeToForget)
        {
            powerful = 0; //if the unit has not seen the player in 50 seconds the power will be set to 0
        }

        
        if (desireList.Max() == powerful)
        {
            if (agent.gameObject.GetComponent<Perception>().MemoryMap != null && agent.gameObject.GetComponent<Perception>().MemoryMap.ContainsKey(player) && agent.gameObject.GetComponent<Perception>().MemoryMap[player].WithinFoV == false)
            {
                //if the powerful state has the highest desirability and the AI cannot see the player, the AI will search for the player
                if(Search_State_On == false)
                {
                    State_Search_Player pSearchState = new State_Search_Player();
                    ResetBools();
                    Search_State_On = true;
                    agent.ChangeState(pSearchState);
                }
                
            }
            else if (agent.gameObject.GetComponent<Perception>().MemoryMap != null && agent.gameObject.GetComponent<Perception>().MemoryMap.ContainsKey(player) && agent.gameObject.GetComponent<Perception>().MemoryMap[player].WithinFoV == true)
            {
                if (player.GetComponent<Player_Gun>().playerPower <= powerful) //checks to see how strong the player is compared to the unit - will make them flee if they feel weaker in comparison
                {
                    if(Vector3.Distance(agent.transform.position, player.transform.position) > agent.fireRange)
                    {
                        if (Search_State_On == false)
                        {
                            State_Search_Player pSearchState = new State_Search_Player();
                            ResetBools();
                            Search_State_On = true;
                            agent.ChangeState(pSearchState);
                        }
                    }
                    else
                    {
                        if (Fight_State_On == false)
                        {
                            State_Fight_Player fightState = new State_Fight_Player();
                            ResetBools();
                            Fight_State_On = true;
                            agent.ChangeState(fightState);
                        }
                    }
             
                   
                }
                else if (player.GetComponent<Player_Gun>().playerPower > powerful)
                {
                    Debug.Log("Epic");
                    if(Flee_State_On == false)
                    {
                        State_Flee fleeState = new State_Flee();
                        ResetBools();
                        Flee_State_On = true;
                        agent.ChangeState(fleeState);
                    }
                
                }

            }
        }
        if (desireList.Max() == weaponDesire)
        {
            //if the desire to find ammo is the highest it will try to find ammo
            if(Ammo_State_On == false)
            {
                State_Search_Ammo ammoState = new State_Search_Ammo();
                ammoState.hasCreatedPath = false;
                ResetBools();
                Ammo_State_On = true;
                agent.ChangeState(ammoState);
            }
          
        }
        if (desireList.Max() == hpDesire)
        {
            //if the desire to find health is the highest it will try to find health
            if (HP_State_On == false)
            {
                State_Search_Health healthState = new State_Search_Health();
                healthState.hasCreatedHealthPath = false;
                ResetBools();
                HP_State_On = true;
                agent.ChangeState(healthState);
            }
           
        }
        if (desireList.Max() == wanderDesire)
        {
            State_Patrol patrolState = new State_Patrol();
            agent.ChangeState(patrolState);
        }
        if (desireList.Max() == postDesire)
        {
            if (Vector3.Distance(transform.position, postLocation) < 5f)
            {
                agent.SB.IsSeekOn = false;
                State_Idle idleState = new State_Idle();
                agent.ChangeState(idleState);
            }
            else
            {
                if(Post_State_On == false)
                {
                    State_Return_To_Post postState = new State_Return_To_Post();
                    ResetBools();
                    Post_State_On = true;
                    agent.ChangeState(postState);
                }
               
            }
            
        }

    }

    //functions to calculate the desirability values:

    public void GetHealthDesirability(float maxHP, float currentHP, float itemDist)
    {
        hpStatus = currentHP / maxHP;

        hpDesire = k * ((1 - hpStatus) / itemDist);
        hpDesire = Mathf.Clamp(hpDesire, 0, 1);
        hpDesire = hpDesire * runModifier;
    }

    public void GetWeaponDesirability(float maxHP, float currentHP, float currentAmmo, float maxAmmo, float weaponDist)
    {
        float weaponAmmo = currentAmmo / maxAmmo;
        if (weaponAmmo == 1)
        {
            weaponDesire = 0;
           
        }
        else
        {
            hpStatus = currentHP / maxHP;
            weaponDesire = k * ((hpStatus * (1 - weaponAmmo)) / weaponDist);
            weaponDesire = Mathf.Clamp(weaponDesire, 0, 1);
            weaponDesire = weaponDesire * runModifier;
        }
    
    }

    public void GetPower(float currentAmmo, float maxAmmo, float currentHealth, float maxHealth)
    {
        float health = currentHealth / maxHealth;
        float totalWeaponAmmo = currentAmmo / maxAmmo;
        powerful = k * totalWeaponAmmo * health;
       // Debug.Log("Powerful is " + totalWeaponAmmo + " * " + health);
        powerful = Mathf.Clamp(powerful, 0, 1);
        //Debug.Log("Powerful comes out as " + powerful);
        powerful = (powerful * fightModifier) - deadAllyCounter;
        //Debug.Log("Powerful after modifier is " + powerful);
    }

    public void ResetBools()
    {
        //resets all of the boolean values that stop the states from being changed 
       HP_State_On = false; //checks to see if it is in the state it is trying to transition to already to avoid constantly creating a new version of the state
       Flee_State_On = false;
       Ammo_State_On = false;
       Search_State_On = false; //for the "player search" state
       Fight_State_On = false;
       Chase_State_On = false;
       Post_State_On = false;
    }

}
