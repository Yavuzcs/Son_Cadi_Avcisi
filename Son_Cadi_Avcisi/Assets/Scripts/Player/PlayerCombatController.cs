using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    //Combat özellikleri
    [SerializeField]
    private bool combatEnabled; // Savaþ modunun etkin olup olmadýðýný belirleyen deðiþken
    [SerializeField]
    private float inputTimer, attack1Radius, attack1Damage; //Giriþ zamaný, saldýrý menzili ve saldýrý hasarý
    [SerializeField]
    private Transform attack1HitBoxPos; //Saldýrý vuruþ kutusunun pozisyonu
    [SerializeField]
    private LayerMask whatIsDamageable; //Zarar verebilen nesnelerin katmanlarý

    private bool gotInput, isAttacking, isFirstAttack; //Giriþ alýndý mý, saldýrý durumu, ilk saldýrý mý kontrol deðiþkenleri

    private float lastInputTime = Mathf.NegativeInfinity; //Son giriþ zamaný

    private float[] attackDetails = new float[2]; //Saldýrý detaylarý (örneðin, saldýrý hasarý, saldýrý yapan objenin x pozisyonu)

    private Animator anim; // Animator bileþeni

    private PlayerController PC; //Oyuncu kontrolcüsü
    private PlayerStats PS; //Oyuncu istatistikleri



    private void Start()
    {
        anim = GetComponent<Animator>(); //Animator bileþenini al
        anim.SetBool("canAttack", combatEnabled); //Animator'a savaþ yapabilme durumunu bildir
        PC = GetComponent<PlayerController>(); //Oyuncu kontrolcüsüne eriþim saðla
        PS = GetComponent<PlayerStats>(); // Oyuncu istatistiklerine eriþim saðla
    }

    private void Update()
    {
        CheckCombatInput(); //Savaþ giriþlerini kontrol etme
        CheckAttacks(); //Saldýrýlarý kontrol etme
    }

    private void CheckCombatInput()
    {
        //Sol mouse tuþuna basýldýðýnda
        if (Input.GetMouseButtonDown(0))
        {
            if (combatEnabled)
            {
                // Savaþý baþlatmaya çalýþ
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (gotInput)
        {
            //Saldýrý1'i gerçekleþtir
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
        //Giriþ zamanýndan belirlenen süre geçtiyse
        if (Time.time >= lastInputTime + inputTimer)
        {
            //Yeni giriþi bekleyin
            gotInput = false;
        }
    }

    private void CheckAttackHitBox()
    {
        //Belirli bir pozisyon ve yarýçap içindeki zarar verebilen nesneleri bulma
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attack1HitBoxPos.position, attack1Radius, whatIsDamageable);

        attackDetails[0] = attack1Damage;//Saldýrý hasarýný belirle
        attackDetails[1] = transform.position.x;//Saldýrý yapan objenin x pozisyonunu belirle

        //Tespit edilen nesneler üzerinde döngü
        foreach (Collider2D collider in detectedObjects)
        {
            //Zarar verme fonksiyonunu çaðýrma
            collider.transform.parent.SendMessage("Damage", attackDetails);
            //Hit particle'ýný oluþturma
        }
    }

    private void FinishAttack1()
    {
        //Saldýrý1 tamamlandýðýnda
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }

    private void Damage(float[] attackDetails)
    {
        //Dash durumu aktif deðilse
        if (!PC.GetDashStatus())
        {
            int direction;
            //Saðlýðý azaltma
            PS.DecreaseHealth(attackDetails[0]);
            //Saldýrýyý yapan objenin pozisyonuna göre itme yönlendirme
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
        //Saldýrý vuruþ kutusunu görselleþtirme
        Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }

}
