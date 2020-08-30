using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Perception : MonoBehaviour
{
  
    // Data from this map is never removed . This gives the AI a "memory" of things it's sensed
    // It's up to you to decide how far into the past the AI remembers things

    public Dictionary<GameObject, MemoryRecord> MemoryMap = new Dictionary<GameObject, MemoryRecord>();

    //For debugging
    public GameObject[] SensedObjects;
    public MemoryRecord[] SensedRecord;

    // Clears all the current FoVs
    public void ClearFoV()
    {
        foreach(KeyValuePair<GameObject, MemoryRecord> Memory in MemoryMap)
        {
            Memory.Value.WithinFoV = false;
        }
    }

    public void AddMemory(GameObject LeTarget)
    {
        //Create a new memory record
        MemoryRecord record = new MemoryRecord();
        if(LeTarget.tag == "Player")
        {
           record = new MemoryRecord(DateTime.Now, LeTarget.transform.position, true, LeTarget.GetComponent<Player_Gun>().currentHP, LeTarget.GetComponent<Player_Gun>().currentAmmo);
        }
        else
        {
            record = new MemoryRecord(DateTime.Now, LeTarget.transform.position, true, LeTarget.GetComponent<Agent>().isDead);
   
        }
       

        //Check if there already is a previous memory record for this target
        if(MemoryMap.ContainsKey(LeTarget))
        {
            //Overwrite the previous record instead of adding a new one
            MemoryMap[LeTarget] = record;
        }
        else
        {
            //Otherwise add the new record
            MemoryMap.Add(LeTarget, record);
        }
    }
    


    void Update()
    {
        //Just expose the values to inspector here so I can see if it's working
        SensedObjects = new GameObject[MemoryMap.Keys.Count];
        SensedRecord = new MemoryRecord[MemoryMap.Values.Count];
        MemoryMap.Keys.CopyTo(SensedObjects, 0);
        MemoryMap.Values.CopyTo(SensedRecord, 0);
    }
}

[Serializable]
public class MemoryRecord
{

    // The time the target was last sensed

    [SerializeField]
    public DateTime TimeLastSensed;


    // The position the target was last sensed

    [SerializeField]
    public Vector3 LastSensedPosition;
 

    // Whether the target is currently within the FoV

    [SerializeField]
    public bool WithinFoV;
    //the hp of the target the last time we saw them
    [SerializeField]
    public float targetHP;

    //the ammo remaining of the target the last time we saw them
    [SerializeField]
    public float targetAmmo;
    //stores whether the target is dead, used for detecting fallen allies
    [SerializeField]
    public bool targetDead;
    public MemoryRecord()
    {
        TimeLastSensed = DateTime.MinValue;
        LastSensedPosition = Vector3.zero;
        
        WithinFoV = false;
        targetHP = float.MinValue;
        targetAmmo = int.MinValue;
    }

    public MemoryRecord(DateTime LeTime, Vector3 LePos, bool LeFoV, float LeHP, float LeAmmo)
    {
        TimeLastSensed = LeTime;
        LastSensedPosition = LePos;
        
        WithinFoV = LeFoV;
        targetHP = LeHP;
        targetAmmo = LeAmmo;
    }
    public MemoryRecord(DateTime LeTime, Vector3 LePos, bool LeFoV, bool isDead)
    {
        TimeLastSensed = LeTime;
        LastSensedPosition = LePos;

        WithinFoV = LeFoV;
        targetDead = isDead;
    }

}
