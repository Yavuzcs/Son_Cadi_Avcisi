using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OyunuDurdur : MonoBehaviour
{
    // Oyun durmu� mu kontrol etmek i�in bir de�i�ken
    private bool oyunDurduMu = false;



    void Update()
    {
        // Oyunu durdurmak veya devam ettirmek i�in "P" tu�una basma kontrol�
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (oyunDurduMu)
            {
                DevamEt();
            }
            else
            {
                Durdur();
            }
        }
    }

    void Durdur()
    {
        // Oyunu durdur
        Time.timeScale = 0;
        oyunDurduMu = true;
    }

    void DevamEt()
    {
        // Oyunu devam ettir
        Time.timeScale = 1;
        oyunDurduMu = false;
    }
}
