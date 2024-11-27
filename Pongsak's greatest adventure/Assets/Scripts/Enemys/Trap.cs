using MyInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Trap : MonoBehaviour
{
    private bool OnTrapTrigger = false;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public bool StartTrap()
    {
        if (!OnTrapTrigger)
        {
            OnTrapTrigger = true;

            animator.SetTrigger("Activate");
            return true;    
        }

        return false;
    }

    public void TrapOff()
    {
        OnTrapTrigger = false;
        animator.SetTrigger("Deactivate");
    }
    private void OnTriggerEnter(Collider other)
    {
       if (OnTrapTrigger)
        {
            if (other.GetComponent<TakeDamage>() != null)
            {
                other.GetComponent<TakeDamage>().TakeDamage(20);
            } 
       }
    }
    void Update()
    {
        
    }
}
