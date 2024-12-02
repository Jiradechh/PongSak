using MyInterface;
using System.Collections;
using UnityEngine;

public class PowerOfDoritos : MonoBehaviour
{
    private Rigidbody rigidbody3D;
    private Collider Collider;

    public Collider DmCollider;

    public bool OnAttack = true;
    private int moveSpeedUp = 1;
    private int moveSpeedAttack = 3;

    public SetControl setControl;

    private bool OnBossAttack = false;

    [HideInInspector] public float duration = 3f;
    void Start()
    {
        rigidbody3D = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
        Collider.enabled = false;

      
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") && !OnBossAttack )
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
                other.GetComponent<PlayerHealth>().KnockBack();
            }
        }
    }

    public void CallReadyToAttack()
    {
        StartCoroutine(ReadyToAttack());
    }

    IEnumerator ReadyToAttack()
    {
        Vector3 currentTargetUp = transform.position; // ¡
        currentTargetUp.y += 2;
        while (transform.position != currentTargetUp)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTargetUp, moveSpeedUp * Time.deltaTime);
            yield return true;
        }
        // setUp
        DmCollider.enabled=true;
        OnAttack = true;
        Collider.enabled = false;
        OnBossAttack = true;
        rigidbody3D = this.gameObject.AddComponent<Rigidbody>();
        rigidbody3D.useGravity = false;
        yield return new WaitForSeconds(3);
        
        Vector3 currentTargetPlayer = PlayerController.Instance.transform.position; // �ԧ
        
        while (transform.position != currentTargetPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPlayer, moveSpeedAttack * Time.deltaTime);
            yield return true;
        }

        setControl.RubikDestory(this);

        Destroy(gameObject);
    }
}
