using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredeeEnemy : MonoBehaviour , IDamageable
{

    public enum EnemyState
    {
        Idle,
        Walk,
        Attack,
        Hurt,
        Dead
    }

    #region Public Variables
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 10;
    public float attackRange = 0.22f;
    public float attackCooldown = 3f;
    public float walkSpeed = 2f;
    public Transform player;
    public Animator animator;
    public LayerMask playerLayer;
    public Rigidbody rb;
    public bool isDead = false;
    public SpriteRenderer spriteRenderer;


    public EnemyState currentState;
    #endregion

    #region Private Variables
    private float attackCooldownTimer;
    private bool isAttacking = false;
    private bool isHurt = false;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        currentHealth = maxHealth;
        attackCooldownTimer = attackCooldown;
        currentState = EnemyState.Idle;  

       
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("Player not found! Make sure the Player object has the 'Player' tag.");
            }
        }

       
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (isDead || isHurt) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

      
        if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0f)
        {
            SetState(EnemyState.Attack);
        }
       
        else if (distanceToPlayer > attackRange)
        {
            SetState(EnemyState.Walk);
            WalkTowardsPlayer();
        }

        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }
    #endregion

    #region State Management
    private void SetState(EnemyState newState)
    {
        if (currentState == newState) return;  

        currentState = newState;

       
        switch (currentState)
        {
            case EnemyState.Walk:
                animator.Play("E3_Walk");
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Hurt:
                animator.Play("E3_Hurt");
                break;
            case EnemyState.Dead:
                animator.Play("E3_Dead");
                Die();
                break;
            case EnemyState.Idle:
            default:
                
                animator.SetBool("isWalking", false);
                break;
        }
    }
    #endregion

    #region Attack Logic
    private void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        animator.Play("E3_Attack");
        attackCooldownTimer = attackCooldown;

        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
        foreach (var hitCollider in hitColliders)
        {
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("PredeeEnemy dealt " + damage + " damage to the player.");
            }
        }

       
        Invoke("EndAttack", 1f);
    }

    private void EndAttack()
    {
        isAttacking = false;
        SetState(EnemyState.Idle); 
    }
    #endregion

    #region Movement Logic
    private void WalkTowardsPlayer()
    {
       
        if (isAttacking) return;

      
        Vector3 direction = (player.position - transform.position).normalized;

       
        if (direction.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = false;
        }

        
        rb.MovePosition(transform.position + direction * walkSpeed * Time.deltaTime);
    }

    private void StopWalking()
    {
        SetState(EnemyState.Idle);
    }
    #endregion

    #region Hurt and Death Logic
    public void TakeDamage(int damage)
    {
        if (isDead) return;

      
        currentHealth -= damage;
        SetState(EnemyState.Hurt);
        isHurt = true;

        if (currentHealth <= 0)
        {
            SetState(EnemyState.Dead);
        }
        else
        {
            
            Invoke("EndHurt", 0.5f);
        }
    }

    private void EndHurt()
    {
        isHurt = false;
        SetState(EnemyState.Idle);
    }

    private void Die()
    {
        isDead = true;
        
        Invoke("DestroyEnemy", 1f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Gizmos
  
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}