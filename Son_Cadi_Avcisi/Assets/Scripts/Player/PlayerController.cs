using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    //Oyuncu hareketi ile ilgili de�i�kenler
    private float movementInputDirection;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;
    private float dashTimeLeft; 
    private float lastImageXpos; 
    private float lastDash = -100f; 
    private float knockbackStartTime;
    [SerializeField]
    private float knockbackDuration;

    //Z�plama
    private int amountOfJumpsLeft;
    private int facingDirection = 1;
    private int lastWallJumpDirection;

    //Oyuncunun bulundu�u durumlar i�in boole veri tipi
    private bool isFacingRight = true;
    private bool isWalking; 
    private bool isGrounded;
    private bool isTouchinWall;
    private bool isWallSliding;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isAttemptingToJump;
    private bool checkJumpMultiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;
    private bool isTouchingLedge;
    private bool canClimbLedge = false;
    private bool ledgeDetected;
    private bool isDashing; 
    private bool knockback;

    [SerializeField]
    private Vector2 knockbackSpeed;

    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;

    private Rigidbody2D rb;
    private Animator anim;

    public int amountOfJumps = 1;

    public float movementSpeed = 10.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float wallHopForce;
    public float wallJumpForce;
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f;
    public float wallJumpTimerSet = 0.5f;
    public float dashTime; 
    public float dashSpeed; 
    public float distanceBetweenImage; 
    public float dashCoolDown; 


    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;

    //duvar atlama y�n�
    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    //zemin, duvar ve ��k�nt�lar� tespit etmek i�in kontrol noktalar�
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform ledgeCheck;

    //Oyuncu yerde mi diye kontrol etme
    public LayerMask whatIsGround;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    //kare ba��na 1 kez �a��r�l�r
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
        CheckLedgeClimb();
        CheckDash();
        CheckKnockback();
    }

    //Hareket uygulamak ve �evreyi kontrol etmek i�in
    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }
    //duvarda kayma yap�yor mu, yapm�yor mu
    private void CheckIfWallSliding()
    {
        if(isTouchinWall && movementInputDirection == facingDirection && rb.velocity.y < 0 && !canClimbLedge)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    public bool GetDashStatus()
    {
        return isDashing;
    }

    public void Knockback(int direction)
    {
        knockback = true;
        knockbackStartTime = Time.time;
        rb.velocity = new Vector2(knockbackSpeed.x * direction, knockbackSpeed.y);
    }
    //geri itmenin etkin olup olmad���n� kontrol eder ve oyuncunun h�z�n� s�f�rlar
    private void CheckKnockback()
    {
        if(Time.time >= knockbackStartTime + knockbackDuration && knockback)
        {
            knockback = false;
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }
    }
    
    //kenar alg�lamay� kontrol eder ve t�rmanmay� ayarlar
    private void CheckLedgeClimb()
    {
        if(ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

            if (isFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }

            canMove = false;
            canFlip = false;

            anim.SetBool("canClimbLedge", canClimbLedge);

        }
        if (canClimbLedge)
        {
            transform.position = ledgePos1; 
        }
        
    }
    //��k�nt�ya t�rmanmay� bitirir, konumlar� s�f�rlar ve hareketi ve takla atmay� etkinle�tirir
    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        anim.SetBool("canClimbLedge", canClimbLedge);
    }

    public void CheckSurroundings()
    {
        //Oyuncunun yerde mi, duvara m� yoksa bir ��k�nt�ya m� dokundu�unu kontrol eder
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchinWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        //Bir duvara dokundu�unuzda ��k�nt� alg�lamay� g�nceller
        if (isTouchinWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
    }
    //Oyuncunun normal bir atlama, duvardan atlama veya bitkin atlama yap�p yapamayaca��n� kontrol eder
    private void CheckIfCanJump()
    {
        if(isGrounded && rb.velocity.y <= 0.01f)
        {
            amountOfJumpsLeft = amountOfJumps;
        }
        if (isTouchinWall)
        {
            canWallJump = true;
        }

        if(amountOfJumpsLeft <= 0)
        {
            canNormalJump = false;
        }

        else
        {
            canNormalJump = true;
        }
    }

    private void CheckMovementDirection()
    {
        //Ters y�nde hareket ederse oynat�c�y� �evirir
        if (isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if(!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        //Yatay h�za g�re y�r�me durumunu ayarlar
        if (Mathf.Abs(rb.velocity.x) >= 0.01f)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    private void UpdateAnimations()
    {
        //Animat�r parametrelerini oynat�c� durumuna g�re g�nceller
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    //Hareket, atlama ve at�lma i�in oyuncu girdilerini y�netir
    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            if(isGrounded || (amountOfJumpsLeft > 0 && isTouchinWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }

        if(Input.GetButtonDown("Horizontal") && isTouchinWall)
        {
            if(!isGrounded && movementInputDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if (turnTimer >= 0)
        {
            turnTimer -= Time.deltaTime;

            if(turnTimer <= 0)
            {
                canMove = true;
                canFlip = true; 
            }
        }
        //Atlama d��mesi b�rak�ld���nda de�i�ken atlama y�ksekli�ini y�netir
        if (checkJumpMultiplier && !Input.GetButton("Jump"))
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
        //Dash'i ba�lat�r
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (Time.time >= (lastDash + dashCoolDown))
            AttempToDash();
        }
    }
    
    private void AttempToDash()
    {
        //Dash'i ba�lat�r
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        //Bir g�r�nt� sonras� efekti yarat�r
        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXpos = transform.position.x;
    }

    public int GetFacingDirection()
    {
        //Oynat�c�n�n ge�erli bakma y�n�n� d�nd�r�r
        return facingDirection;
    }

    private void CheckDash()
    {
        //�izgi mekani�ini y�netir
        if (isDashing)
        {
            if(dashTimeLeft > 0)
            {
                canMove = false;
                canFlip = false;
                rb.velocity = new Vector2(dashSpeed * facingDirection, 0);
                dashTimeLeft -= Time.deltaTime;

                if(Mathf.Abs(transform.position.x - lastImageXpos) > distanceBetweenImage)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXpos = transform.position.x;
                }
            }

            if(dashTimeLeft <= 0 || isTouchinWall)
            {
                isDashing = false;
                canMove = true;
                canFlip = true;
            }
        }
    }

    private void CheckJump()
    {
        //D�zenli atlamalar ve duvar atlamalar�n�n mant���n� y�netir
        if (jumpTimer > 0)
        {
            //Atlama zamanlay�c�s� s�ras�nda duvar atlama durumunu kontrol eder
            if (!isGrounded && isTouchinWall && movementInputDirection != 0 && movementInputDirection != facingDirection)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                NormalJump();
            }
            //Atlamaya �al���rken atlama zamanlay�c�s�n� azalt�r
            if (isAttemptingToJump)
            {
                jumpTimer -= Time.deltaTime;
            }
        }
        //Duvardan atlama kurtarma zamanlay�c�s�n� y�netir
        if (wallJumpTimer > 0)
        {
            if(hasWallJumped && movementInputDirection == -lastWallJumpDirection)
            {
                //Duvar atlamas�ndan sonra kalan dikey h�z� iptal eder
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJumped = false;
            }
            else if(wallJumpTimer <= 0)
            {
                //Zamanlay�c� bitti�inde duvar atlama durumunu s�f�rlar
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }

        
    }
    //Ko�ullar kar��lan�rsa normal bir s��rama ger�ekle�tirir
    private void NormalJump()
    {
        if (canNormalJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }
    //Ko�ullar kar��lan�rsa duvardan atlama ger�ekle�tirir
    private void WallJump()
    {
        if (canWallJump)
        {
            //Duvar kaymas�n� iptal eder ve atlamayla ilgili parametreleri s�f�rlar
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            amountOfJumpsLeft = amountOfJumps;
            amountOfJumpsLeft--;
            //Duvardan atlama i�in kuvvet uygular
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            //Atlamayla ilgili durumlar� ve zamanlay�c�lar� s�f�rlar
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    private void ApplyMovement()
    {
        //Oyuncunun durumuna g�re hareket kuvvetleri uygular
        if (!isGrounded && !isWallSliding && movementInputDirection == 0 && !knockback)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
        else if(canMove && !knockback)
        {
            //Geri itme alt�nda olmad���nda normal hareket kuvveti uygular
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }

        //Duvar kayma hareketini y�netir
        if (isWallSliding)
        {
            if(rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }
    
    public void DisableFlip()
    {
        //�evirme yetene�ini devre d��� b�rak�r
        canFlip = false;
    }

    public void EnableFlip()
    {
        //�evirme yetene�ini etkinle�tirir
        canFlip = true;
    }
    //Oyuncu karakterini �evirir ve bakan y�n� g�nceller
    private void Flip()
    {
        if (!isWallSliding && canFlip && !knockback)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }

    }
    //Unity edit�r�nde zemin ve duvar kontrolleri i�in g�rselle�tirme �izer
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }


}
