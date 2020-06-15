using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Singleton = null; // The current EvolutionManager Instance

    [SerializeField] bool UseNodeMutation = true; // Should we use node mutation?

    public const int Population = 100; // The number of cars per generation
    [SerializeField] GameObject CarPrefab; // The Prefab of the car to be created for each instance
    [SerializeField] Text GenerationNumberText; // Some text to write the generation number

    int GenerationCount = 0; // The current generation number

    List<CharacterBehaviour> Cars = new List<CharacterBehaviour>(); // This list of cars currently alive

    NeuralNetwork BestNeuralNetwork = null; // The best NeuralNetwork currently available
    int BestFitness = -1; // The FItness of the best NeuralNetwork ever created

    private LevelSpawner _spawner;
    private CheckpointController2 _checkpointController2;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var multilevelController = GetComponent<MultilevelController>();
        
        if (multilevelController.playMode != MultilevelController.GamePlayMode.GA
            || scene.buildIndex < 1)
            return;

        if (Singleton == null) // If no other instances were created
            Singleton = this; // Make the only instance this one
        else
            gameObject.SetActive(false); // There is another instance already in place. Make this one inactive.

        BestNeuralNetwork = new NeuralNetwork(Car.NextNetwork); // Set the BestNeuralNetwork to a random new network
        _spawner = multilevelController.levelSpawner;
        _checkpointController2 = _spawner?.GetComponentInChildren<CheckpointController2>();

        StartGeneration();
    }

    // Sarts a whole new generation
    void StartGeneration()
    {
        GenerationCount++; // Increment the generation count
        GenerationNumberText.text = "Generation: " + GenerationCount; // Update generation text
        //print(GenerationCount);

        GetComponent<MultilevelController>().allCharacters.gameObject.SetActive(true);
        for (int i = 0; i < Population; i++)
        {
            if (i == 0)
                Car.NextNetwork = BestNeuralNetwork; // Make sure one car uses the best network
            else
            {
                Car.NextNetwork =
                    new NeuralNetwork(
                        BestNeuralNetwork); // Clone the best neural network and set it to be for the next car
                if (UseNodeMutation) // Should we use Node Mutation
                    Car.NextNetwork.MutateNodes(); // Mutate its nodes
                else
                    Car.NextNetwork.Mutate(); // Mutate its weights
            }

//            Cars.Add(Instantiate(CarPrefab, transform.position, Quaternion.identity, transform).GetComponent<CharacterBehaviour>()); // Instantiate a new car and add it to the list of cars
            if (ReferenceEquals(_checkpointController2, null))
                return;
//            _checkpointController2.LocateCheckpoints(); //TODO: necessary?
            Cars.Add(_spawner.NewPair());
        }

        GetComponent<MultilevelController>().allCharacters.gameObject.SetActive(false);
    }

    // Gets called by cars when they die
    public void CarDead(AllCharactersInfo DeadCar, int Fitness)
    {
        Cars.Remove(DeadCar.playerBehaviour); // Remove the car from the list
        _spawner.minimap.RemoveCharacter(DeadCar.playerMover);
        _spawner.minimap.RemoveCharacter(DeadCar.npcMover);

        Destroy(DeadCar.gameObject); // Destroy the dead car

        if (Fitness > BestFitness) // If it is better that the current best car
        {
            BestNeuralNetwork = DeadCar.playerBehaviour.TheNetwork; // Make sure it becomes the best car
            BestFitness = Fitness; // And also set the best fitness
        }

        if (Cars.Count <= 0) // If there are no cars left
            StartGeneration(); // Create a new generation
    }
}