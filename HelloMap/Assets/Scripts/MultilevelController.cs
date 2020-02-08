using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MultilevelController : MonoBehaviour
{
    [SerializeField] public AllCharactersInfo allCharacters;
    
    [SerializeField] private float timeScale = 1f;

    [NonSerialized] public LevelSpawner levelSpawner;

    public GamePlayMode playMode = GamePlayMode.Normal;

    private int trainingStart = Int32.Parse(File.ReadAllLines("last_output.log")[0]);

    public bool randomisedPositioning = false;

    public enum GamePlayMode
    {
        Normal,
        GA,
        NN,
        NN_REC
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        allCharacters.gameObject.SetActive(true);
        levelSpawner = GameObject.FindGameObjectWithTag("LevelCoordinates")
                                 ?.GetComponent<LevelSpawner>();
        if (ReferenceEquals(levelSpawner, null))
            return;
        levelSpawner.ConfigureLevel(allCharacters);
        if (playMode == GamePlayMode.Normal || playMode == GamePlayMode.NN_REC || playMode == GamePlayMode.NN)
            levelSpawner.MoveExistingPair(randomisedPositioning);

        if (playMode == GamePlayMode.NN_REC)
            DataContainer.activated = true;
//        GeneticAlgorithm.population = allPlayers;
    }

    private void Update()
    {
        Time.timeScale = timeScale;
        if (Input.GetKey(KeyCode.X))
        {
            File.WriteAllText("last_output.log", (++trainingStart).ToString());
            File.Move("output.csv", $"output_{trainingStart}.csv");
//            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
        
        if (Input.GetKey(KeyCode.C))
        {
            File.Delete("output.csv");
//            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }
}