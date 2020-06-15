using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] bool UseUserInput = false; // Defines whether the car uses a NeuralNetwork or user input
    [SerializeField] LayerMask SensorMask; // Defines the layer of the walls ("Wall")
    [SerializeField] float FitnessUnchangedDie = 5; // The number of seconds to wait before checking if the fitness didn't increase
    
    public static NeuralNetwork NextNetwork = new NeuralNetwork(new uint[] { 6, 4, 3, 2 }, null); // public NeuralNetwork that refers to the next neural network to be set to the next instantiated car

    public string TheGuid { get; private set; } // The Unique ID of the current car

    public int Fitness { get; private set; } // The fitness/score of the current car. Represents the number of checkpoints that his car hit.

    public NeuralNetwork TheNetwork { get; private set; } // The NeuralNetwork of the current car

    Rigidbody TheRigidbody; // The Rigidbody of the current car
    LineRenderer TheLineRenderer; // The LineRenderer of the current car
    
    private void Awake()
    {
        TheGuid = Guid.NewGuid().ToString(); // Assigns a new Unique ID for the current car
        
        TheNetwork = NextNetwork; // Sets the current network to the Next Network
        NextNetwork = new NeuralNetwork(NextNetwork.Topology, null); // Make sure the Next Network is reassigned to avoid having another car use the same network

        TheRigidbody = GetComponent<Rigidbody>(); // Assign Rigidbody
        TheLineRenderer = GetComponent<LineRenderer>(); // Assign LineRenderer

        StartCoroutine(IsNotImproving()); // Start checking if the score stayed the same for a lot of time

        TheLineRenderer.positionCount = 17; // Make sure the line is long enough
    }

    private void FixedUpdate()
    {
        if (UseUserInput) // If we're gonna use user input
            Move(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal")); // Moves the car according to the input
        else // if we're gonna use a neural network
        {
            float Vertical;
            float Horizontal;

            GetNeuralInputAxis(out Vertical, out Horizontal);

            Move(Vertical, Horizontal); // Moves the car
        }
    }

    // Casts all the rays, puts them through the NeuralNetwork and outputs the Move Axis
    void GetNeuralInputAxis (out float Vertical, out float Horizontal)
    {
        double[] NeuralInput = new double[NextNetwork.Topology[0]]; //TODO: lookat

        // Cast forward, back, right and left
        NeuralInput[0] = CastRay(transform.forward, Vector3.forward, 1) / 4;
        NeuralInput[1] = CastRay(-transform.forward, -Vector3.forward, 3) / 4;
        NeuralInput[2] = CastRay(transform.right, Vector3.right, 5) / 4;
        NeuralInput[3] = CastRay(-transform.right, -Vector3.right, 7) / 4;

        // Cast forward-right and forward-left
        float SqrtHalf = Mathf.Sqrt(0.5f);
        NeuralInput[4] = CastRay(transform.right * SqrtHalf + transform.forward * SqrtHalf, Vector3.right * SqrtHalf + Vector3.forward * SqrtHalf, 9) / 4;
        NeuralInput[5] = CastRay(transform.right * SqrtHalf + -transform.forward * SqrtHalf, Vector3.right * SqrtHalf + -Vector3.forward * SqrtHalf, 13) / 4;

        // Feed through the network
        double[] NeuralOutput = TheNetwork.FeedForward(NeuralInput);
        
        // Get Vertical Value
        if (NeuralOutput[0] <= 0.25f)
            Vertical = -1;
        else if (NeuralOutput[0] >= 0.75f)
            Vertical = 1;
        else
            Vertical = 0;

        // Get Horizontal Value
        if (NeuralOutput[1] <= 0.25f)
            Horizontal = -1;
        else if (NeuralOutput[1] >= 0.75f)
            Horizontal = 1;
        else
            Horizontal = 0;

        // If the output is just standing still, then move the car forward
        if (Vertical == 0 && Horizontal == 0)
            Vertical = 1;
    }

    // Checks each few seconds if the car didn't make any improvement
    IEnumerator IsNotImproving ()
    {
        while(true)
        {
            int OldFitness = Fitness; // Save the initial fitness
            yield return new WaitForSeconds(FitnessUnchangedDie); // Wait for some time
            if (OldFitness == Fitness) // Check if the fitness didn't change yet
                WallHit(); // Kill this car
        }
    }

    // Casts a ray and makes it visible through the line renderer
    double CastRay (Vector3 RayDirection, Vector3 LineDirection, int LinePositionIndex)
    {
        float Length = 4; // Maximum length of each ray

        RaycastHit Hit;
        if (Physics.Raycast(transform.position, RayDirection, out Hit, Length, SensorMask)) // Cast a ray
        {
            float Dist = Vector3.Distance(Hit.point, transform.position); // Get the distance of the hit in the line
            TheLineRenderer.SetPosition(LinePositionIndex, Dist * LineDirection); // Set the position of the line

            return Dist; // Return the distance
        }
        else
        {
            TheLineRenderer.SetPosition(LinePositionIndex, LineDirection * Length); // Set the distance of the hit in the line to the maximum distance

            return Length; // Return the maximum distance
        }
    }
    
    // The main function that moves the car.
    public void Move (float v, float h)
    {
        TheRigidbody.velocity = transform.right * v * 4;
        TheRigidbody.angularVelocity = transform.up * h * 3;
    }

    // This function is called through all the checkpoints when the car hits any.
    public void CheckpointHit ()
    {
        Fitness++; // Increase Fitness/Score
    }

    // Called by walls when hit by the car
    public void WallHit()
    {
//        EvolutionManager.Singleton.CarDead(this, Fitness); // Tell the Evolution Manager that the car is dead
//        gameObject.SetActive(false); // Make sure the car is inactive
    }
}
