using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    #region Public Variables
    public float moveSpeed = 5f;
    public float dashSpeed = 5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public int maxDashes = 2;
    public Rigidbody rb;
    public bool canMove = true;
    public Transform playerTransform;
    public Animator animator;
    public ParticleSystem walkParticleSystem;

    public Transform circleIndicator;

    public static bool isUIActive = false;
    public GameObject smokeTrail;
    #endregion

    #region Private Variables
    private Vector3 moveDirection;
    private Vector3 smoothMoveDirection;
    private bool isDashing = false;
    private float dashTimer;
    private float dashCooldownTimer;
    private int remainingDashes;
    private SpriteRenderer spriteRenderer;
    private bool dashOnCooldown = false;
    private bool isMoving;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        remainingDashes = maxDashes;

        circleIndicator.rotation = Quaternion.Euler(90, 0, -90);

        if (walkParticleSystem != null)
        {
            walkParticleSystem.Stop();
        }
    }

    void Update()
    {
        Debug.Log(rb.velocity);
        if (ShopSystemGold.IsShopOpen || ShopSystemGold.IsPopupActive || ShopSystem.IsShopOpen)
        {
            
            return;
        }

        if (canMove)
        {
            HandleInput();
            HandleDash();
        }

        UpdateAnimation();
        UpdateCircleIndicator();
    }

    void FixedUpdate()
    {
        if (isUIActive || !canMove || isDashing) return;
        MovePlayer();
    }
    #endregion

    #region Input Handling
    private void HandleInput()
    {
        if (isUIActive) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
        isMoving = moveDirection.magnitude > 0;

        HandleSpriteFlip(moveX);

        if (isMoving)
        {
            PlayWalkParticles();
            SoundManager.Instance.PlayWalkSound();
        }
        else
        {
            StopWalkParticles();
            SoundManager.Instance.StopWalkSound();
        }
    }

    private void HandleSpriteFlip(float moveX)
    {
        spriteRenderer.flipX = moveX < 0;

       
        if (smokeTrail != null)
        {
            Vector3 smokeTrailRotation = smokeTrail.transform.eulerAngles;
            smokeTrailRotation.y = moveX < 0 ? 180f : 0f;
            smokeTrail.transform.eulerAngles = smokeTrailRotation;
        }
    }
    #endregion

    #region Movement Logic
    private void MovePlayer()
    {
        smoothMoveDirection = Vector3.Lerp(smoothMoveDirection, moveDirection, Time.fixedDeltaTime * 10f);
        rb.MovePosition(transform.position + smoothMoveDirection * moveSpeed * Time.fixedDeltaTime);
    }
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        Debug.Log($"Player's move speed set to {moveSpeed}");
    }
    #endregion

    #region Dash Logic
    private void HandleDash()
    {
        if ((Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Space)) && remainingDashes > 0 && !isDashing)
        {
            StartCoroutine(Dash());
        }
        if (remainingDashes == 0 && !dashOnCooldown)
        {
            StartCoroutine(RefillDashes());
        }
    }

    //private IEnumerator Dash()
    //{
    //    isDashing = true;
    //    dashTimer = dashTime;
    //    PlayerHealth.Instance.isInvincible = true;
    //    animator.ResetTrigger("Dash");
    //    animator.SetTrigger("Dash");

    //    remainingDashes--;

    //    while (dashTimer > 0)
    //    {
    //        rb.MovePosition(transform.position + smoothMoveDirection * dashSpeed * Time.deltaTime);
    //        dashTimer -= Time.deltaTime;
    //        yield return null;
    //    }
    //    isDashing = false;
    //    PlayerHealth.Instance.isInvincible = false;
    //}
    private IEnumerator Dash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        PlayerHealth.Instance.isInvincible = true;
        animator.ResetTrigger("Dash");
        animator.SetTrigger("Dash");

        SoundManager.Instance.PlayDashSound();

        remainingDashes--;
        Vector3 dashDirection = circleIndicator.transform.up;
        rb.AddForce(dashDirection * dashSpeed, ForceMode.Impulse);
        yield return new WaitForSeconds(dashDuration);
        rb.Sleep();
        rb.velocity = Vector3.zero;
        isDashing = false;
        PlayerHealth.Instance.isInvincible = false;
    }
    private IEnumerator RefillDashes()
    {
        dashOnCooldown = true;
        yield return new WaitForSeconds(dashCooldown);
        remainingDashes = maxDashes;
        dashOnCooldown = false;
    }


    #endregion

    #region Dash Upgrade
    private const float DefaultDashSpeed = 5f;
    public void IncreaseMaxDashes()
    {
        maxDashes++;
        remainingDashes = maxDashes;
        Debug.Log("Player's max dashes increased to " + maxDashes);
    }
    public void DecreaseMaxDashes()
    {
        if (maxDashes > 1)
        {
            maxDashes = 1;
            remainingDashes = maxDashes;
            Debug.Log("Player's max dashes decreased to 1.");
        }
    }
    public void IncreaseDashSpeed(float amount)
    {
        dashSpeed += amount;
        Debug.Log($"Player's dash speed increased by {amount}. New dash speed: {dashSpeed}");
    }

    public void DecreaseDashSpeed(float amount)
    {
        dashSpeed -= amount;
        if (dashSpeed < DefaultDashSpeed) dashSpeed = DefaultDashSpeed;
        Debug.Log($"Player's dash speed decreased by {amount}. New dash speed: {dashSpeed}");
    }

    public void ResetDashSpeed()
    {
        dashSpeed = DefaultDashSpeed;
        Debug.Log($"Player's dash speed reset to default: {DefaultDashSpeed}");
    }
    #endregion

    #region Animation and Particle Logic
    private void UpdateAnimation()
    {
        animator.SetBool("isWalkingX", Mathf.Abs(moveDirection.x) > 0);
        animator.SetBool("isWalkingZ", Mathf.Abs(moveDirection.z) > 0);
        animator.SetBool("isDashing", isDashing);
    }

    private void PlayWalkParticles()
    {
        if (walkParticleSystem != null && !walkParticleSystem.isPlaying)
        {
            walkParticleSystem.Play();
        }
    }

    private void StopWalkParticles()
    {
        if (walkParticleSystem != null && walkParticleSystem.isPlaying)
        {
            walkParticleSystem.Stop();
        }
    }

    public void ResetControllerState()
    {
        canMove = true;
        isDashing = false;
        dashCooldownTimer = 0f;
        remainingDashes = maxDashes;
        dashOnCooldown = false;
        animator.Play("Idle");
        StopWalkParticles();
    }

    #endregion

    #region Circle Indicator Logic
    private void UpdateCircleIndicator()
    {
        Vector3 facingDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        if (facingDirection.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(facingDirection.z, facingDirection.x) * Mathf.Rad2Deg;
            circleIndicator.rotation = Quaternion.Euler(90, 0, angle - 90);
        }
        else
        {
            circleIndicator.rotation = Quaternion.Euler(90, 0, -90);
        }
    }
    #endregion
}