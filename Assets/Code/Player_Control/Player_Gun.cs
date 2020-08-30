using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Player_Gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 70f;

    public float fireRate = 2f;
    public Camera playerCam;

    public float maxAmmo = 10f;
    public float currentAmmo = 10f;

    public float soundRadius = 200f; //the radius for the area of the map where AI will be able to hear the gunshot go off
    public LayerMask enemyLayer; 
    private float nextTimeToFire = 0f;

    public float currentHP = 50; //will be stored in the gun for ease of access
    public float maxHP = 50;
    public float playerPower = 0;

    public Text healthText; //text that displays how much HP the player has

    void Update()
    {
        healthText.text = currentHP.ToString();
        SetPlayerPower();
        if(Input.GetButton("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            currentAmmo -= 1;
            Debug.Log("Firing");
            Fire();
        }
    }

    void Fire()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, soundRadius);
        for(int i = 0; i < hitColliders.Length; i++)
        {
            if(hitColliders[i].gameObject.tag == "Enemy_AI")
            {
                hitColliders[i].gameObject.GetComponent<Perception>().AddMemory(this.gameObject);
                Debug.Log(hitColliders[i].gameObject.name);
            }
            
        }

        RaycastHit hit;
        if(Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, range))
        {
            if(hit.transform.gameObject.tag == "Enemy_AI")
            {
                hit.transform.gameObject.GetComponent<Agent>().TakeDamage(damage);
                hit.transform.gameObject.GetComponent<Perception>().AddMemory(this.gameObject);
            }
        }

    }

    public void SetPlayerPower()
    {
        float health = currentHP / maxHP;
        float totalWeaponAmmo = currentAmmo / maxAmmo;
        playerPower = totalWeaponAmmo * health;
        playerPower = Mathf.Clamp(playerPower, 0, 1);
      
        
        //will be used to get a grasp of how powerful the player is so that the AI can judge against its own power level to see if it should fight
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
    }
}
