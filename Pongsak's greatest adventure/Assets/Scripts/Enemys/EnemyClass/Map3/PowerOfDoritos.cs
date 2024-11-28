using MyInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerOfDoritos : MonoBehaviour 
{
    private Rigidbody rigidbody3D;
    private Collider Collider;

    public Collider DmCollider;

    public bool OnAttack = true;  


    void Start()
    {
        rigidbody3D = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
        Collider.enabled = false;

        StartCoroutine(RubikDuration());
    }

    void Update()
    {
       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer ==  LayerMask.NameToLayer("Ground"))
        {
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
            rigidbody3D.Sleep();
            rigidbody3D.velocity = Vector3.zero;
            Destroy(rigidbody3D);
            DmCollider.enabled = false;
            OnAttack = false;
            Collider.enabled = true;
        }
        if (OnAttack)
        {
            if (other.GetComponent<TakeDamage>() != null)
            {
                other.GetComponent<TakeDamage>().TakeDamage(20);
                other.GetComponent <PlayerHealth>().KnockBack();
            }
        }
    }

    IEnumerator RubikDuration()
    {
        yield return new WaitForSeconds(12f);
        Destroy(this.gameObject);
    }
}
