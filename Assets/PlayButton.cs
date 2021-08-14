using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public void PlayGame()
    {
      // call next scene
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
