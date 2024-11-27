using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTrap : MonoBehaviour
{
    public Trap trap;


    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!trap.StartTrap())
        {
            trap.StartTrap();
        }
    }

    void Update()
    {
        
    }
}
