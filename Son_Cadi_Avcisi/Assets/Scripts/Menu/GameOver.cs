using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameOver : MonoBehaviour
{


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Finish")
        {
            gameOver();
        }

    }

    void gameOver()
    {

        SceneManager.LoadScene("GameOverScene");

    }
}
