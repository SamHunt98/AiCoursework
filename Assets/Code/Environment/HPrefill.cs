using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPrefill : MonoBehaviour
{
    public BoxCollider refillCollider;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Player_Gun>().currentHP = other.gameObject.GetComponent<Player_Gun>().maxHP;
            Debug.Log("Player has healed");
        }
        if (other.gameObject.tag == "Enemy_AI")
        {
            other.gameObject.GetComponent<Agent>().CurrentHP = other.gameObject.GetComponent<Agent>().MaxHP;
        }

    }
}
