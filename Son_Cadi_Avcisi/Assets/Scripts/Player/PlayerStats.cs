using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Oyuncu saðlýk özellikleri
    [SerializeField]
    private float maxHealth;//Maksimum saðlýk deðeri

    //Ölüm efektleri
    [SerializeField]
    private GameObject
    deathChunkParticle,//Ölüm efekti (parçacýk sistemi)
    deathBloodParticle;// Ölüm efekti (kan parçacýk sistemi)

    private float currentHealth;//Mevcut saðlýk deðeri

    private GameManager GM;//GameManager sýnýfýna eriþim saðlamak için kullanýlan referans

    private void Start()
    {
        currentHealth = maxHealth;//Baþlangýçta mevcut saðlýk deðerini maksimum saðlýk deðeri ile ayarlama
        GM = GameObject.Find("GameManager").GetComponent<GameManager>();//GameManager bileþenine eriþim saðlama
    }

    //Saðlýk azaltma fonksiyonu
    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;// Belirtilen miktarda saðlýðý azalt
        // Eðer saðlýk sýfýra veya daha azýna düþtüyse
        if (currentHealth <= 0.0f)
        {
            Die();// Oyuncuyu öldürme fonksiyonunu çaðýr
        }
    }

    //Ölüm iþlemlerini gerçekleþtiren fonksiyon
    private void Die()
    {
        // Ölüm efektlerini oluþtur
        Instantiate(deathChunkParticle, transform.position, deathChunkParticle.transform.rotation);
        Instantiate(deathBloodParticle, transform.position, deathBloodParticle.transform.rotation);
        GM.Respawn();//GameManager sýnýfýndaki Respawn fonksiyonunu çaðýrarak oyuncuyu yeniden doðurtma
        Destroy(gameObject);//Oyuncu objesini yok etme
    }
}
