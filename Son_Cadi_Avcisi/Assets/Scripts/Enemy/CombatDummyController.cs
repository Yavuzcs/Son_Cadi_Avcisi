using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDummyController : MonoBehaviour
{
    [SerializeField]
    private float maxHealth, knockbackSpeedX, knockbackSpeedY, knockbackDuration, knockbackDeathSpeedX, knockbackDeathSpeedY, deathTorque;
    [SerializeField]
    private bool applyKnockback;
    [SerializeField]
    private GameObject hitParticle;

    private float currentHealth, knockbackStart;

    private int playerFacingDirection;

    private bool playerOnLeft, knockback;


    private PlayerController pc;
    private GameObject aliveGO, brokenTopGO, brokenBotGO;
    private Rigidbody2D rbAlive, rbBrokenTop, rbBrokenBot;
    private Animator aliveAnim;

    private void Start()
    {
        //Ba�lang��ta sa�l�k ve referanslar� ayarlama
        currentHealth = maxHealth;
        // PlayerController bile�enine eri�im sa�lama
        pc = GameObject.Find("Player").GetComponent<PlayerController>();
        //aliveGO, brokenTopGO ve brokenBotGO objelerinin referanslar�n� alma
        aliveGO = transform.Find("Alive").gameObject;
        brokenTopGO = transform.Find("Broken Top").gameObject;
        brokenBotGO = transform.Find("Broken Bottom").gameObject;
        //Bile�enleri alma
        aliveAnim = aliveGO.GetComponent<Animator>();
        rbAlive = aliveGO.GetComponent<Rigidbody2D>();
        rbBrokenTop = brokenTopGO.GetComponent<Rigidbody2D>();
        rbBrokenBot = brokenBotGO.GetComponent<Rigidbody2D>();
        //Objelerin ba�lang��ta durumlar�n� ayarlama
        aliveGO.SetActive(true);
        brokenTopGO.SetActive(false);
        brokenBotGO.SetActive(false);
    }

    private void Update()
    {
        //Knockback durumunu kontrol etme
        CheckKnockback();
    }

    //Oyuncu taraf�ndan hasar alma fonksiyonu
    public void Damage(float[] details)
    {
        //Sa�l��� azaltma
        currentHealth -= details[0];
        //Oyuncunun hangi y�nde oldu�unu belirleme
        if (details[1] < aliveGO.transform.position.x)
        {
            playerFacingDirection = 1;
        }
        else
        {
            playerFacingDirection = -1;
        }
        //Hasar efekti olu�turma
        Instantiate(hitParticle, aliveAnim.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        //Oyuncunun hangi y�nde oldu�unu belirleme
        if (playerFacingDirection == 1)
        {
            playerOnLeft = true;
        }
        else
        {
            playerOnLeft = false;
        }
        //Animator'a bilgi g�nderme
        aliveAnim.SetBool("playerOnLeft", playerOnLeft);
        aliveAnim.SetTrigger("damage");
        //Knockback uygulanacaksa ve can hala varsa Knockback fonksiyonunu �a��rma
        if (applyKnockback && currentHealth > 0.0f)
        {
            //knockback
            Knockback();
        }
        //Can s�f�r veya daha azsa Die fonksiyonunu �a��rma
        if (currentHealth <= 0.0f)
        {
            //die
            Die();
        }
    }

    //Knockback fonksiyonu
    private void Knockback()
    {
        knockback = true;
        knockbackStart = Time.time;
        rbAlive.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY);
    }

    //Knockback durumunu kontrol eden fonksiyon
    private void CheckKnockback()
    {
        if(Time.time >= knockbackStart + knockbackDuration && knockback)
        {
            knockback = false;
            rbAlive.velocity = new Vector2(0.0f, rbAlive.velocity.y);
        }
    }
    //�l�m fonksiyonu
    private void Die()
    {
        //Canl�y� devre d��� b�rak, k�r�k �st� aktif et, k�r�k alt� devre d��� b�rak
        aliveGO.SetActive(false);
        brokenTopGO.SetActive(true);
        brokenBotGO.SetActive(false);
        //K�r�k �st ve k�r�k alt objelerine h�z uygulama
        brokenTopGO.transform.position = aliveGO.transform.position;
        brokenBotGO.transform.position = aliveGO.transform.position;
        //K�r�k �st objesine tork uygulama
        rbBrokenBot.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY);
        rbBrokenTop.velocity = new Vector2(knockbackDeathSpeedX * playerFacingDirection, knockbackDeathSpeedY);
        rbBrokenTop.AddTorque(deathTorque * -playerFacingDirection, ForceMode2D.Impulse);
    }
}
