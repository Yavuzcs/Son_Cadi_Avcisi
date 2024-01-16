using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasicEnemyController : MonoBehaviour
{
    //Durumlarý tanýmlayan enum
    private enum State
    {
        Moving,
        Knockback,
        Dead
    }

    private State currentState;// Mevcut durumu saklayan deðiþken

    [SerializeField]
    private float
        groundCheckDistance,
        wallCheckDistance,
        movementSpeed,
        maxHealth,
        knockbackDuration,
        lastTouchDamageTime,
        touchDamageCooldown,
        touchDamage,
        touchDamageWidth, 
        touchDamageHeight; 
    [SerializeField]
    private Transform
        groundCheck,
        wallCheck,
        touchDamageCheck;
    [SerializeField]
    private LayerMask 
        whatIsGround,
        whatIsPlayer;
    [SerializeField]
    private Vector2 knockbackSpeed;
    [SerializeField]
    private GameObject
        hitParticle,
        deathChunkParticle,
        deathBloodParticle;

    private float 
        currentHealth,
        knockbackStartTime;

    private float[] attackDetails = new float[2];

    private int 
        facingDirection,
        damageDirection;

    private Vector2 
        movement,
        touchDamageBotLeft,
        touchDamageTopRight;



    private bool
        groundDetected,
        wallDetected;

    private GameObject alive;
    private Rigidbody2D aliveRb;
    private Animator aliveAnim;

    private void Start()
    {
        //Referanslarý baþlatma
        alive = transform.Find("Alive").gameObject;
        aliveRb = alive.GetComponent<Rigidbody2D>();
        aliveAnim = alive.GetComponent<Animator>();

        currentHealth = maxHealth;
        facingDirection = 1;
    }

    //Mevcut duruma göre güncelleme yapma
    private void Update()
    {
        switch (currentState)
        {
            case State.Moving:
                UpdateMovingState();
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
        }
    }

    // WALKÝNG STATE ----------------------------

    private void EnterMovingState()
    {

    }

    private void UpdateMovingState()
    {
        //Zemin kontrolü ve hareket etme
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        CheckTouchDamage();
        //Zemin yoksa veya duvar varsa yönlendirmeyi deðiþtirme, aksi halde hareket etme
        if (!groundDetected || wallDetected)
        {
            Flip();
        }
        else
        {
            movement.Set(movementSpeed * facingDirection, aliveRb.velocity.y);
            aliveRb.velocity = movement;
        }
    }


    private void ExitMovingState()
    {

    }

    // KNOCK BAKE STATE-------------------------------------
    
    private void EnterKnockbackState()
    {
        //Knockback state'e geçiþte yapýlacaklar
        knockbackStartTime = Time.time;
        movement.Set(knockbackSpeed.x * damageDirection, knockbackSpeed.y);
        aliveRb.velocity = movement;
        aliveAnim.SetBool("Knockback", true);
    }

    private void UpdateKnockbackState()
    {
        //Belirlenen süre boyunca knockback state'de kalma
        if (Time.time >= knockbackStartTime + knockbackDuration)
        {
            SwitchState(State.Moving);
        }
    }

    private void ExitKnockbackState()
    {
        //Knockback state'den çýkýþta yapýlacaklar
        aliveAnim.SetBool("Knockback", false);
    }


    //DEAD STATE-----------------------------------


    private void EnterDeadState()
    {
        //Dead state'e geçiþte yapýlacaklar
        Instantiate(deathChunkParticle, alive.transform.position, deathChunkParticle.transform.rotation);
        Instantiate(deathBloodParticle, alive.transform.position, deathBloodParticle.transform.rotation);
        Destroy(gameObject);
    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    //OTHER FUNCTÝONS----------------------------------

    private void Damage(float[] attackDetails)
    {
        //Hasar alma iþlemleri
        currentHealth -= attackDetails[0];
        //Hasar efekti oluþturma
        Instantiate(hitParticle, alive.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));
        //Hasarýn yönünü belirleme
        if (attackDetails[1] > alive.transform.position.x)
        {
            damageDirection = -1;
        }
        else
        {
            damageDirection = 1;
        }

        //Can hala varsa Knockback state'e geçiþ, aksi halde Dead state'e geçiþ
        if (currentHealth > 0.0f)
        {
            SwitchState(State.Knockback);
        }
        else if(currentHealth <= 0.0f)
        {
            SwitchState(State.Dead);
        }
    }

    private void CheckTouchDamage()
    {
        //Zamanlayýcý kontrolü ve dokunma hasarý
        if (Time.time >= lastTouchDamageTime + touchDamageCooldown)
        {
            touchDamageBotLeft.Set(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
            touchDamageTopRight.Set(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));

            Collider2D hit = Physics2D.OverlapArea(touchDamageBotLeft, touchDamageTopRight, whatIsPlayer);

            if(hit != null)
            {
                lastTouchDamageTime = Time.time;
                attackDetails[0] = touchDamage;
                attackDetails[1] = alive.transform.position.x;
                hit.SendMessage("Damage", attackDetails);
            }
        }
    }

    private void Flip()
    {
        //Yönü tersine çevirme
        facingDirection *= -1;
        alive.transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void SwitchState(State state)
    {
        //Durum deðiþtirme fonksiyonu
        switch (currentState)
        {
            case State.Moving:
                ExitMovingState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;
            default:
                break;
        }

        switch (state)
        {
            case State.Moving:
                EnterMovingState();
                break;
            case State.Knockback:
                EnterKnockbackState();
                break;
            case State.Dead:
                EnterDeadState();
                break;
            default:
                break;
        }

        currentState = state;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));

        Vector2 botLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
        Vector2 botRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
        Vector2 topRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));
        Vector2 topLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));

        Gizmos.DrawLine(botLeft, botRight);
        Gizmos.DrawLine(botRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, botLeft);
    }
}
