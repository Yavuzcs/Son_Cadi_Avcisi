using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Transform respawnPoint;//Yeniden doðma noktasýný belirlemek için kullanýlan Transform
    [SerializeField]
    private GameObject player;//Oyuncu objesini tutmak için kullanýlan GameObject
    [SerializeField]
    private float respawnTime;//Yeniden doðma süresi

    private float respawnTimeStart;//Yeniden doðma süresinin baþlangýç zamanýný saklayan deðiþken

    private bool respawn;//Oyuncunun yeniden doðup doðmamasýný kontrol etmek için kullanýlan bool

    private CinemachineVirtualCamera CVC;//Cinemachine sanal kamera referansý

    private void Start()
    {
        //Oyuncu kamerasýnýn CinemachineVirtualCamera bileþenini bulma
        CVC = GameObject.Find("Player Camera").GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        //Yeniden doðma durumunu kontrol etme
        CheckRespawn();
    }

    public void Respawn()
    {
        respawnTimeStart = Time.time;//Yeniden doðma süresinin baþlangýç zamanýný kaydetme
        respawn = true;// Yeniden doðma durumunu aktifleþtirme
    }

    //Yeniden doðma süresini kontrol eden fonksiyon
    private void CheckRespawn()
    {
        //Belirlenen süre geçtiyse ve yeniden doðma aktifse
        if (Time.time >= respawnTimeStart + respawnTime && respawn)
        {
            //Oyuncuyu yeniden oluþtur
            var playerTemp = Instantiate(player, respawnPoint);
            //Kameranýn takip etmesi gereken nesneyi güncelleme
            CVC.m_Follow = playerTemp.transform;
            respawn = false;//Yeniden doðma durumunu devre dýþý býrakma
        }
    }
}
