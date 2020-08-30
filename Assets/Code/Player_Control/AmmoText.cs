using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoText : MonoBehaviour
{
    public Player_Gun gun;
    public Text ammoText;

    //updates the text on the screen representing how much ammo the player has
    void Start()
    {
        ammoText = GetComponent<Text>();
        ammoText.text = gun.maxAmmo.ToString();
    }

    void Update()
    {
        ammoText.text = gun.currentAmmo.ToString();
    }
}
