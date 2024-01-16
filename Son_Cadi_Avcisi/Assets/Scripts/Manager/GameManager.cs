using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Transform respawnPoint;//Yeniden do�ma noktas�n� belirlemek i�in kullan�lan Transform
    [SerializeField]
    private GameObject player;//Oyuncu objesini tutmak i�in kullan�lan GameObject
    [SerializeField]
    private float respawnTime;//Yeniden do�ma s�resi

    private float respawnTimeStart;//Yeniden do�ma s�resinin ba�lang�� zaman�n� saklayan de�i�ken

    private bool respawn;//Oyuncunun yeniden do�up do�mamas�n� kontrol etmek i�in kullan�lan bool

    private CinemachineVirtualCamera CVC;//Cinemachine sanal kamera referans�

    private void Start()
    {
        //Oyuncu kameras�n�n CinemachineVirtualCamera bile�enini bulma
        CVC = GameObject.Find("Player Camera").GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        //Yeniden do�ma durumunu kontrol etme
        CheckRespawn();
    }

    public void Respawn()
    {
        respawnTimeStart = Time.time;//Yeniden do�ma s�resinin ba�lang�� zaman�n� kaydetme
        respawn = true;// Yeniden do�ma durumunu aktifle�tirme
    }

    //Yeniden do�ma s�resini kontrol eden fonksiyon
    private void CheckRespawn()
    {
        //Belirlenen s�re ge�tiyse ve yeniden do�ma aktifse
        if (Time.time >= respawnTimeStart + respawnTime && respawn)
        {
            //Oyuncuyu yeniden olu�tur
            var playerTemp = Instantiate(player, respawnPoint);
            //Kameran�n takip etmesi gereken nesneyi g�ncelleme
            CVC.m_Follow = playerTemp.transform;
            respawn = false;//Yeniden do�ma durumunu devre d��� b�rakma
        }
    }
}
