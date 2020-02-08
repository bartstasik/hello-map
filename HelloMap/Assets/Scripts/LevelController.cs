using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public static void NextLevel()
    {
        var nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextScene > SceneManager.sceneCount)
            return;
        SceneManager.LoadScene(nextScene);
    }

    public static void RestartLevel()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex);
    }
    
    public static void ZeroLevel()
    {
        SceneManager.LoadScene(0);
    }

    public static void LastLevel()
    {
        SceneManager.LoadScene("LevelEnd");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        NextLevel();
    }
}