using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OyunuDurdur : MonoBehaviour
{
    // Oyun durmuþ mu kontrol etmek için bir deðiþken
    private bool oyunDurduMu = false;



    void Update()
    {
        // Oyunu durdurmak veya devam ettirmek için "P" tuþuna basma kontrolü
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
