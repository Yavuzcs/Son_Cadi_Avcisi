using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Oyuncu sa�l�k �zellikleri
    [SerializeField]
    private float maxHealth;//Maksimum sa�l�k de�eri

    //�l�m efektleri
    [SerializeField]
    private GameObject
    deathChunkParticle,//�l�m efekti (par�ac�k sistemi)
    deathBloodParticle;// �l�m efekti (kan par�ac�k sistemi)

    private float currentHealth;//Mevcut sa�l�k de�eri

    private GameManager GM;//GameManager s�n�f�na eri�im sa�lamak i�in kullan�lan referans

    private void Start()
    {
        currentHealth = maxHealth;//Ba�lang��ta mevcut sa�l�k de�erini maksimum sa�l�k de�eri ile ayarlama
        GM = GameObject.Find("GameManager").GetComponent<GameManager>();//GameManager bile�enine eri�im sa�lama
    }

    //Sa�l�k azaltma fonksiyonu
    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;// Belirtilen miktarda sa�l��� azalt
        // E�er sa�l�k s�f�ra veya daha az�na d��t�yse
        if (currentHealth <= 0.0f)
        {
            Die();// Oyuncuyu �ld�rme fonksiyonunu �a��r
        }
    }

    //�l�m i�lemlerini ger�ekle�tiren fonksiyon
    private void Die()
    {
        // �l�m efektlerini olu�tur
        Instantiate(deathChunkParticle, transform.position, deathChunkParticle.transform.rotation);
        Instantiate(deathBloodParticle, transform.position, deathBloodParticle.transform.rotation);
        GM.Respawn();//GameManager s�n�f�ndaki Respawn fonksiyonunu �a��rarak oyuncuyu yeniden do�urtma
        Destroy(gameObject);//Oyuncu objesini yok etme
    }
}
