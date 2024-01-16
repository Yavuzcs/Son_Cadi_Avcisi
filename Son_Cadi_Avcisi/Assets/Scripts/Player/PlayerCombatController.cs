using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    //Combat �zellikleri
    [SerializeField]
    private bool combatEnabled; // Sava� modunun etkin olup olmad���n� belirleyen de�i�ken
    [SerializeField]
    private float inputTimer, attack1Radius, attack1Damage; //Giri� zaman�, sald�r� menzili ve sald�r� hasar�
    [SerializeField]
    private Transform attack1HitBoxPos; //Sald�r� vuru� kutusunun pozisyonu
    [SerializeField]
    private LayerMask whatIsDamageable; //Zarar verebilen nesnelerin katmanlar�

    private bool gotInput, isAttacking, isFirstAttack; //Giri� al�nd� m�, sald�r� durumu, ilk sald�r� m� kontrol de�i�kenleri

    private float lastInputTime = Mathf.NegativeInfinity; //Son giri� zaman�

    private float[] attackDetails = new float[2]; //Sald�r� detaylar� (�rne�in, sald�r� hasar�, sald�r� yapan objenin x pozisyonu)

    private Animator anim; // Animator bile�eni

    private PlayerController PC; //Oyuncu kontrolc�s�
    private PlayerStats PS; //Oyuncu istatistikleri



    private void Start()
    {
        anim = GetComponent<Animator>(); //Animator bile�enini al
        anim.SetBool("canAttack", combatEnabled); //Animator'a sava� yapabilme durumunu bildir
        PC = GetComponent<PlayerController>(); //Oyuncu kontrolc�s�ne eri�im sa�la
        PS = GetComponent<PlayerStats>(); // Oyuncu istatistiklerine eri�im sa�la
    }

    private void Update()
    {
        CheckCombatInput(); //Sava� giri�lerini kontrol etme
        CheckAttacks(); //Sald�r�lar� kontrol etme
    }

    private void CheckCombatInput()
    {
        //Sol mouse tu�una bas�ld���nda
        if (Input.GetMouseButtonDown(0))
        {
            if (combatEnabled)
            {
                // Sava�� ba�latmaya �al��
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (gotInput)
        {
            //Sald�r�1'i ger�ekle�tir
            if (!isAttacking)
            {
                gotInput = false;
                isAttacking = true;
                isFirstAttack = !isFirstAttack;
                anim.SetBool("attack1", true);
                anim.SetBool("firstAttack", isFirstAttack);
                anim.SetBool("isAttacking", isAttacking);
            }
        }
        //Giri� zaman�ndan belirlenen s�re ge�tiyse
        if (Time.time >= lastInputTime + inputTimer)
        {
            //Yeni giri�i bekleyin
            gotInput = false;
        }
    }

    private void CheckAttackHitBox()
    {
        //Belirli bir pozisyon ve yar��ap i�indeki zarar verebilen nesneleri bulma
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attack1HitBoxPos.position, attack1Radius, whatIsDamageable);

        attackDetails[0] = attack1Damage;//Sald�r� hasar�n� belirle
        attackDetails[1] = transform.position.x;//Sald�r� yapan objenin x pozisyonunu belirle

        //Tespit edilen nesneler �zerinde d�ng�
        foreach (Collider2D collider in detectedObjects)
        {
            //Zarar verme fonksiyonunu �a��rma
            collider.transform.parent.SendMessage("Damage", attackDetails);
            //Hit particle'�n� olu�turma
        }
    }

    private void FinishAttack1()
    {
        //Sald�r�1 tamamland���nda
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }

    private void Damage(float[] attackDetails)
    {
        //Dash durumu aktif de�ilse
        if (!PC.GetDashStatus())
        {
            int direction;
            //Sa�l��� azaltma
            PS.DecreaseHealth(attackDetails[0]);
            //Sald�r�y� yapan objenin pozisyonuna g�re itme y�nlendirme
            if (attackDetails[1] < transform.position.x)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }
            //Oyuncuyu geri itme
            PC.Knockback(direction);
        }

    }


    private void OnDrawGizmos()
    {
        //Sald�r� vuru� kutusunu g�rselle�tirme
        Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }

}
