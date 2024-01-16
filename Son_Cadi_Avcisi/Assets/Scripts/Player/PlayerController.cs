using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    //Oyuncu hareketi ile ilgili deðiþkenler
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

    //Zýplama
    private int amountOfJumpsLeft;
    private int facingDirection = 1;
    private int lastWallJumpDirection;

    //Oyuncunun bulunduðu durumlar için boole veri tipi
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

    //duvar atlama yönü
    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    //zemin, duvar ve çýkýntýlarý tespit etmek için kontrol noktalarý
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

    //kare baþýna 1 kez çaðýrýlýr
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

    //Hareket uygulamak ve çevreyi kontrol etmek için
    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }
    //duvarda kayma yapýyor mu, yapmýyor mu
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
    //geri itmenin etkin olup olmadýðýný kontrol eder ve oyuncunun hýzýný sýfýrlar
    private void CheckKnockback()
    {
        if(Time.time >= knockbackStartTime + knockbackDuration && knockback)
        {
            knockback = false;
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }
    }
    
    //kenar algýlamayý kontrol eder ve týrmanmayý ayarlar
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
    //Çýkýntýya týrmanmayý bitirir, konumlarý sýfýrlar ve hareketi ve takla atmayý etkinleþtirir
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
        //Oyuncunun yerde mi, duvara mý yoksa bir çýkýntýya mý dokunduðunu kontrol eder
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchinWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        //Bir duvara dokunduðunuzda çýkýntý algýlamayý günceller
        if (isTouchinWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
    }
    //Oyuncunun normal bir atlama, duvardan atlama veya bitkin atlama yapýp yapamayacaðýný kontrol eder
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
        //Ters yönde hareket ederse oynatýcýyý çevirir
        if (isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if(!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        //Yatay hýza göre yürüme durumunu ayarlar
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
        //Animatör parametrelerini oynatýcý durumuna göre günceller
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    //Hareket, atlama ve atýlma için oyuncu girdilerini yönetir
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
        //Atlama düðmesi býrakýldýðýnda deðiþken atlama yüksekliðini yönetir
        if (checkJumpMultiplier && !Input.GetButton("Jump"))
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
        //Dash'i baþlatýr
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (Time.time >= (lastDash + dashCoolDown))
            AttempToDash();
        }
    }
    
    private void AttempToDash()
    {
        //Dash'i baþlatýr
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        //Bir görüntü sonrasý efekti yaratýr
        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXpos = transform.position.x;
    }

    public int GetFacingDirection()
    {
        //Oynatýcýnýn geçerli bakma yönünü döndürür
        return facingDirection;
    }

    private void CheckDash()
    {
        //Çizgi mekaniðini yönetir
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
        //Düzenli atlamalar ve duvar atlamalarýnýn mantýðýný yönetir
        if (jumpTimer > 0)
        {
            //Atlama zamanlayýcýsý sýrasýnda duvar atlama durumunu kontrol eder
            if (!isGrounded && isTouchinWall && movementInputDirection != 0 && movementInputDirection != facingDirection)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                NormalJump();
            }
            //Atlamaya çalýþýrken atlama zamanlayýcýsýný azaltýr
            if (isAttemptingToJump)
            {
                jumpTimer -= Time.deltaTime;
            }
        }
        //Duvardan atlama kurtarma zamanlayýcýsýný yönetir
        if (wallJumpTimer > 0)
        {
            if(hasWallJumped && movementInputDirection == -lastWallJumpDirection)
            {
                //Duvar atlamasýndan sonra kalan dikey hýzý iptal eder
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJumped = false;
            }
            else if(wallJumpTimer <= 0)
            {
                //Zamanlayýcý bittiðinde duvar atlama durumunu sýfýrlar
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }

        
    }
    //Koþullar karþýlanýrsa normal bir sýçrama gerçekleþtirir
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
    //Koþullar karþýlanýrsa duvardan atlama gerçekleþtirir
    private void WallJump()
    {
        if (canWallJump)
        {
            //Duvar kaymasýný iptal eder ve atlamayla ilgili parametreleri sýfýrlar
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            amountOfJumpsLeft = amountOfJumps;
            amountOfJumpsLeft--;
            //Duvardan atlama için kuvvet uygular
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            //Atlamayla ilgili durumlarý ve zamanlayýcýlarý sýfýrlar
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
        //Oyuncunun durumuna göre hareket kuvvetleri uygular
        if (!isGrounded && !isWallSliding && movementInputDirection == 0 && !knockback)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
        else if(canMove && !knockback)
        {
            //Geri itme altýnda olmadýðýnda normal hareket kuvveti uygular
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }

        //Duvar kayma hareketini yönetir
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
        //çevirme yeteneðini devre dýþý býrakýr
        canFlip = false;
    }

    public void EnableFlip()
    {
        //Çevirme yeteneðini etkinleþtirir
        canFlip = true;
    }
    //Oyuncu karakterini çevirir ve bakan yönü günceller
    private void Flip()
    {
        if (!isWallSliding && canFlip && !knockback)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }

    }
    //Unity editöründe zemin ve duvar kontrolleri için görselleþtirme çizer
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }


}
