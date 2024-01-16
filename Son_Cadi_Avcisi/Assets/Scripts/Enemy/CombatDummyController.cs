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
        //Baþlangýçta saðlýk ve referanslarý ayarlama
        currentHealth = maxHealth;
        // PlayerController bileþenine eriþim saðlama
        pc = GameObject.Find("Player").GetComponent<PlayerController>();
        //aliveGO, brokenTopGO ve brokenBotGO objelerinin referanslarýný alma
        aliveGO = transform.Find("Alive").gameObject;
        brokenTopGO = transform.Find("Broken Top").gameObject;
        brokenBotGO = transform.Find("Broken Bottom").gameObject;
        //Bileþenleri alma
        aliveAnim = aliveGO.GetComponent<Animator>();
        rbAlive = aliveGO.GetComponent<Rigidbody2D>();
        rbBrokenTop = brokenTopGO.GetComponent<Rigidbody2D>();
        rbBrokenBot = brokenBotGO.GetComponent<Rigidbody2D>();
        //Objelerin baþlangýçta durumlarýný ayarlama
        aliveGO.SetActive(true);
        brokenTopGO.SetActive(false);
        brokenBotGO.SetActive(false);
    }

    private void Update()
    {
        //Knockback durumunu kontrol etme
        CheckKnockback();
    }

    //Oyuncu tarafýndan hasar alma fonksiyonu
    public void Damage(float[] details)
    {
        //Saðlýðý azaltma
        currentHealth -= details[0];
        //Oyuncunun hangi yönde olduðunu belirleme
        if (details[1] < aliveGO.transform.position.x)
        {
            playerFacingDirection = 1;
        }
        else
        {
            playerFacingDirection = -1;
        }
        //Hasar efekti oluþturma
        Instantiate(hitParticle, aliveAnim.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        //Oyuncunun hangi yönde olduðunu belirleme
        if (playerFacingDirection == 1)
        {
            playerOnLeft = true;
        }
        else
        {
            playerOnLeft = false;
        }
        //Animator'a bilgi gönderme
        aliveAnim.SetBool("playerOnLeft", playerOnLeft);
        aliveAnim.SetTrigger("damage");
        //Knockback uygulanacaksa ve can hala varsa Knockback fonksiyonunu çaðýrma
        if (applyKnockback && currentHealth > 0.0f)
        {
            //knockback
            Knockback();
        }
        //Can sýfýr veya daha azsa Die fonksiyonunu çaðýrma
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
    //Ölüm fonksiyonu
    private void Die()
    {
        //Canlýyý devre dýþý býrak, kýrýk üstü aktif et, kýrýk altý devre dýþý býrak
        aliveGO.SetActive(false);
        brokenTopGO.SetActive(true);
        brokenBotGO.SetActive(false);
        //Kýrýk üst ve kýrýk alt objelerine hýz uygulama
        brokenTopGO.transform.position = aliveGO.transform.position;
        brokenBotGO.transform.position = aliveGO.transform.position;
        //Kýrýk üst objesine tork uygulama
        rbBrokenBot.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY);
        rbBrokenTop.velocity = new Vector2(knockbackDeathSpeedX * playerFacingDirection, knockbackDeathSpeedY);
        rbBrokenTop.AddTorque(deathTorque * -playerFacingDirection, ForceMode2D.Impulse);
    }
}
