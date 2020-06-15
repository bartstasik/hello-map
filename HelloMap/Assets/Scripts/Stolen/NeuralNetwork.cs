using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Random = System.Random;

public class NeuralNetwork
{
    public UInt32[] Topology // Returns the topology in the form of an array
    {
        get
        {
            UInt32[] Result = new UInt32[TheTopology.Count];

            TheTopology.CopyTo(Result, 0);

            return Result;
        }
    }

    ReadOnlyCollection<UInt32> TheTopology; // Contains the topology of the NeuralNetwork
    NeuralSection[] Sections; // Contains the all the sections of the NeuralNetwork

    Random TheRandomizer; // It is the Random instance used to mutate the NeuralNetwork

    private class NeuralSection
    {
        private double[][]
            Weights; // Contains all the weights of the section where [i][j] represents the weight from neuron i in the input layer and neuron j in the output layer

        private Random TheRandomizer; // Contains a reference to the Random instance of the NeuralNetwork

        /// <summary>
        /// Initiate a NeuralSection from a topology and a seed.
        /// </summary>
        /// <param name="InputCount">The number of input neurons in the section.</param>
        /// <param name="OutputCount">The number of output neurons in the section.</param>
        /// <param name="Randomizer">The Ransom instance of the NeuralNetwork.</param>
        public NeuralSection(UInt32 InputCount, UInt32 OutputCount, Random Randomizer)
        {
            // Validation Checks
            if (InputCount == 0)
                throw new ArgumentException("You cannot create a Neural Layer with no input neurons.", "InputCount");
            else if (OutputCount == 0)
                throw new ArgumentException("You cannot create a Neural Layer with no output neurons.", "OutputCount");
            else if (Randomizer == null)
                throw new ArgumentException("The randomizer cannot be set to null.", "Randomizer");

            // Set Randomizer
            TheRandomizer = Randomizer;

            // Initialize the Weights array
            Weights = new double[InputCount + 1][]; // +1 for the Bias Neuron

            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = new double[OutputCount];

            // Set random weights
            for (int i = 0; i < Weights.Length; i++)
            for (int j = 0; j < Weights[i].Length; j++)
                Weights[i][j] = TheRandomizer.NextDouble() - 0.5f;
        }

        /// <summary>
        /// Initiates an independent Deep-Copy of the NeuralSection provided.
        /// </summary>
        /// <param name="Main">The NeuralSection that should be cloned.</param>
        public NeuralSection(NeuralSection Main)
        {
            // Set Randomizer
            TheRandomizer = Main.TheRandomizer;

            // Initialize Weights
            Weights = new double[Main.Weights.Length][];

            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = new double[Main.Weights[0].Length];

            // Set Weights
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    Weights[i][j] = Main.Weights[i][j];
                }
            }
        }

        /// <summary>
        /// Feed input through the NeuralSection and get the output.
        /// </summary>
        /// <param name="Input">The values to set the input neurons.</param>
        /// <returns>The values in the output neurons after propagation.</returns>
        public double[] FeedForward(double[] Input, string activation)
        {
            // Validation Checks
            if (Input == null)
                throw new ArgumentException("The input array cannot be set to null.", "Input");
            else if (Input.Length != Weights.Length - 1)
                throw new ArgumentException(
                    "The input array's length does not match the number of neurons in the input layer.", "Input");

            // Initialize Output Array
            double[] Output = new double[Weights[0].Length];

            // Calculate Value
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    if (i == Weights.Length - 1) // If is Bias Neuron
                        Output[j] += Weights[i][j]; // Then, the value of the neuron is equal to one
                    else
                        Output[j] += Weights[i][j] * Input[i];
                }
            }

            // Apply Activation Function
            switch (activation)
            {
                case "relu":
                    for (int i = 0; i < Output.Length; i++)
                        Output[i] = ReLU(Output[i]);
                    break;
                case "tanh":
                    for (int i = 0; i < Output.Length; i++)
                        Output[i] = HyperbolicTangtent(Output[i]);
                    break;
                default:
                    for (int i = 0; i < Output.Length; i++)
                        Output[i] = ReLU(Output[i]);
                    break;
            }

            // Return Output
            return Output;
        }

        /// <summary>
        /// Mutate the NeuralSection.
        /// </summary>
        /// <param name="MutationProbablity">The probability that a weight is going to be mutated. (Ranges 0-1)</param>
        /// <param name="MutationAmount">The maximum amount a Mutated Weight would change.</param>
        public void Mutate(double MutationProbablity, double MutationAmount)
        {
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    if (TheRandomizer.NextDouble() < MutationProbablity)
//                        Weights[i][j] = TheRandomizer.NextDouble() * (MutationAmount * 2) - MutationAmount;
                        Weights[i][j] = Weights[i][j] + (MutationAmount * TheRandomizer.Next(-1, 2));
                }
            }
        }

        /// <summary>
        /// Mutate the NeuralSection's Nodes.
        /// </summary>
        /// <param name="MutationProbablity">The probability that a node is going to be mutated. (Ranges 0-1)</param>
        /// <param name="MutationAmount">The maximum amount a Mutated Weight would change.</param>
        public void MutateNodes(double MutationProbablity, double MutationAmount)
        {
            for (int j = 0; j < Weights[0].Length; j++) // For each output node
            {
                if (TheRandomizer.NextDouble() < MutationProbablity) // Check if we are going to mutate this node
                {
                    for (int i = 0; i < Weights.Length; i++) // For each input node connected to the current output node
                    {
                        Weights[i][j] =
                            TheRandomizer.NextDouble() * (MutationAmount * 2) -
                            MutationAmount; // Mutate the weight connecting both nodes
                    }
                }
            }
        }

        /// <summary>
        /// Puts a double through the activation function ReLU.
        /// </summary>
        /// <param name="x">The value to put through the function.</param>
        /// <returns>x after it is put through ReLU.</returns>
        private double ReLU(double x)
        {
            if (x >= 0)
                return x;
            else
                return x / 20;
        }

        public double HyperbolicTangtent(double x)
        {
            if (x < -45.0) return -1.0;
            else if (x > 45.0) return 1.0;
            else return Math.Tanh(x);
        }
    }

    /// <summary>
    /// Initiates a NeuralNetwork from a Topology and a Seed.
    /// </summary>
    /// <param name="Topology">The Topology of the Neural Network.</param>
    /// <param name="Seed">The Seed of the Neural Network. Set to 'null' to use a Timed Seed.</param>
    public NeuralNetwork(UInt32[] Topology, Int32? Seed = 0)
    {
        // Validation Checks
        if (Topology.Length < 2)
            throw new ArgumentException("A Neural Network cannot contain less than 2 Layers.", "Topology");

        for (int i = 0; i < Topology.Length; i++)
        {
            if (Topology[i] < 1)
                throw new ArgumentException("A single layer of neurons must contain, at least, one neuron.",
                                            "Topology");
        }

        // Initialize Randomizer
        if (Seed.HasValue)
            TheRandomizer = new Random(Seed.Value);
        else
            TheRandomizer = new Random();

        // Set Topology
        TheTopology = new List<uint>(Topology).AsReadOnly();

        // Initialize Sections
        Sections = new NeuralSection[TheTopology.Count - 1];

        // Set the Sections
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i] = new NeuralSection(TheTopology[i], TheTopology[i + 1], TheRandomizer);
        }
    }

    /// <summary>
    /// Initiates an independent Deep-Copy of the Neural Network provided.
    /// </summary>
    /// <param name="Main">The Neural Network that should be cloned.</param>
    public NeuralNetwork(NeuralNetwork Main)
    {
        // Initialize Randomizer
        TheRandomizer = new Random(Main.TheRandomizer.Next());

        // Set Topology
        TheTopology = Main.TheTopology;

        // Initialize Sections
        Sections = new NeuralSection[TheTopology.Count - 1];

        // Set the Sections
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i] = new NeuralSection(Main.Sections[i]);
        }
    }

    /// <summary>
    /// Feed Input through the NeuralNetwork and Get the Output.
    /// </summary>
    /// <param name="Input">The values to set the Input Neurons.</param>
    /// <returns>The values in the output neurons after propagation.</returns>
    public double[] FeedForward(double[] Input)
    {
        // Validation Checks
        if (Input == null)
            throw new ArgumentException("The input array cannot be set to null.", "Input");
        else if (Input.Length != TheTopology[0])
            throw new ArgumentException(
                "The input array's length does not match the number of neurons in the input layer.", "Input");

        double[] Output = Input;

        // Feed values through all sections
        for (int i = 0; i < Sections.Length - 1; i++)
        {
            Output = Sections[i].FeedForward(Output, "relu");
        }
        
        Output = Sections[Sections.Length - 1].FeedForward(Output, "tanh");

        return Output;
    }

    /// <summary>
    /// Mutate the NeuralNetwork.
    /// </summary>
    /// <param name="MutationProbablity">The probability that a weight is going to be mutated. (Ranges 0-1)</param>
    /// <param name="MutationAmount">The maximum amount a mutated weight would change.</param>
    public void Mutate(double MutationProbablity = 0.3, double MutationAmount = 2.0)
    {
        // Mutate each section
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i].Mutate(MutationProbablity, MutationAmount);
        }
    }

    /// <summary>
    /// Mutate the NeuralNetwork's Nodes.
    /// </summary>
    /// <param name="MutationProbablity">The probability that a node is going to be mutated. (Ranges 0-1)</param>
    /// <param name="MutationAmount">The maximum amount a Mutated Weight would change.</param>
    public void MutateNodes(double MutationProbablity = 0.3, double MutationAmount = 2.0)
    {
        // Mutate each section
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i].MutateNodes(MutationProbablity, MutationAmount);
        }
    }
}