using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    
    public States currentState; //stores the state that the agent is currently in
    public State_Manager managerState;
    public bool canMove = true; //to see whether steering behaviours should be added to stop any unwanted movements during idle
    public float MaxHP; //the default hp of the unit and the highest it can possibly go - can be used to calculate a % of remaining hp. cannot be changed
    public float CurrentHP; //stores the current value of the HP at any given time.
    public bool isDead = false; //stores whether the unit has been killed or not
    //HP distance variables
    public float hpDistance; //the distance to the nearest HP replenishment station
    public GameObject closestHp;
    public float ammoDistance;
    public GameObject closestAmmo;
    public int maxAmmo = 10;
    public int currentAmmo;
    public float fireRate = 1f;
    public float gunDamage = 10f;
   
    //target visibility
    public bool isTargetVisible;
    public Vector3 playerLastSeen; //stores the value for the last place the AI saw the player, allowing them to chase towards it if searching
    public Vector3 playerLastDirection; //gets the forward vector of the player the last time they were seen, meaning  that it can calculate what direction they were moving in

    public float fireRange; //range of the weapon - if outside of this range the ai should pursue into it until it can fire.
    public SteeringBehaviours SB;

    public bool shouldFollowPath = false;

    void Start()
    {
        managerState = GetComponentInChildren<State_Manager>();
        currentState = new State_Patrol();
        SB = GetComponent<SteeringBehaviours>();
        CurrentHP = MaxHP;
        currentAmmo = maxAmmo;
        FindNearestHP();
    }


    void Update()
    {
        if(!isDead)
        {
            FindNearestHP();
            FindNearestAmmo();
            managerState.GetHealthDesirability(MaxHP, CurrentHP, DesireDistanceToItem());
            managerState.GetWeaponDesirability(MaxHP, CurrentHP, currentAmmo, maxAmmo, DesireDistanceToAmmo());
            managerState.GetPower(currentAmmo, maxAmmo, CurrentHP, MaxHP);
            currentState.Execute(this); //executes the current state that is assigned to it
        }

        if (CurrentHP <= 0 && isDead == false)
        {
            EnemyDie();
        }
        
    }
    public void ChangeState(States newState)
    {
        currentState = newState;
    }
    public void FindNearestHP()
    {
        GameObject[] healthPacks;
        healthPacks = GameObject.FindGameObjectsWithTag("Health_Pack");
        closestHp = null;
        hpDistance = Mathf.Infinity;
        foreach (GameObject hp in healthPacks)
        {
            float curDistance = Vector3.Distance(hp.transform.position, transform.position);
            
            if (curDistance < hpDistance)
            {
                closestHp = hp;
                hpDistance = curDistance;
                
            }
        }
    }
    public void FindNearestAmmo()
    {
        GameObject[] ammoPacks;
        ammoPacks = GameObject.FindGameObjectsWithTag("Ammo_Pack");
        closestAmmo = null;
        ammoDistance = Mathf.Infinity;
        foreach (GameObject ammo in ammoPacks)
        {
            float curDistance = Vector3.Distance(ammo.transform.position, transform.position);

            if (curDistance < ammoDistance)
            {
                closestAmmo = ammo;
                ammoDistance = curDistance;

            }
        }
    }
    public float DesireDistanceToItem()
    {
       float desire = 1;
       if(hpDistance >= 150)
        {
            desire = 1;

        }
       else if(hpDistance < 150 & hpDistance >= 120)
        {
            desire = 0.8f;
        }
     
        else if (hpDistance < 120 & hpDistance >= 100)
        {
            desire = 0.6f;
        }
        else if (hpDistance < 100 & hpDistance >= 80)
        {
            desire = 0.4f;
        }
        else if (hpDistance < 80 & hpDistance >= 50)
        {
            desire = 0.2f;
        }
       else  if (hpDistance < 50 & hpDistance >= 0)
        {
            desire = 0;
        }

        return desire;
    }
    public float DesireDistanceToAmmo()
    {
        float desire = 1;
        if (ammoDistance >= 150)
        {
            desire = 1;

        }
        else if (ammoDistance < 150 & ammoDistance >= 120)
        {
            desire = 0.8f;
        }

        else if (ammoDistance < 120 & ammoDistance >= 100)
        {
            desire = 0.6f;
        }
        else if (ammoDistance < 100 & ammoDistance >= 80)
        {
            desire = 0.4f;
        }
        else if (ammoDistance < 80 & ammoDistance >= 50)
        {
            desire = 0.2f;
        }
        else if (ammoDistance < 50 & ammoDistance >= 0)
        {
            desire = 0;
        }

        return desire;
    }
    public void TakeDamage(float damage)
    {
        CurrentHP -= damage;
        if(CurrentHP <= 0)
        {
            EnemyDie();
        }
    }
    void EnemyDie()
    {
        this.gameObject.GetComponent<Vehicle>().enabled = false;
        this.gameObject.GetComponent<SteeringBehaviours>().enabled = false;
        this.gameObject.GetComponent<FieldOfView>().enabled = false;
        this.gameObject.GetComponent<Perception>().enabled = false;
        this.gameObject.GetComponentInChildren<State_Manager>().enabled = false;
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z);
        gameObject.transform.Rotate(90, 0, 0);
        isDead = true;

    }
}
