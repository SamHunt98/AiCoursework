using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoRefill : MonoBehaviour
{
    public BoxCollider refillCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Player_Gun>().currentAmmo = other.gameObject.GetComponent<Player_Gun>().maxAmmo;
            Debug.Log("Player has healed");
        }
        if (other.gameObject.tag == "Enemy_AI")
        {
            other.gameObject.GetComponent<Agent>().currentAmmo = other.gameObject.GetComponent<Agent>().maxAmmo;
        }

    }
}
