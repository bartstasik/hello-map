using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MultilevelController : MonoBehaviour
{
    [SerializeField] private GameObject allCharacters;
    [SerializeField] private GameObject playerMover;
    [SerializeField] private GameObject npc;
    [SerializeField] private GameObject npcMover;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        allCharacters.SetActive(true);
        GameObject.FindGameObjectWithTag("LevelCoordinates")?
                  .GetComponent<LevelCoordinates>()
                  .ConfigureLevel(allCharacters);
    }
}