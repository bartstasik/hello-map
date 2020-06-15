using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class CharacterBehaviour : MonoBehaviour
{
    public enum Type
    {
        Player,
        NPC,
        GA_NPC,
        NN_NPC
    }

    public Type characterType = Type.NPC;

    public float runSpeed = 5;
    public float sprintSpeed = 10;
    public float jumpSpeed = 3;
    public float lookSpeed = 5;
    public float rotationSpeed = 10;

    [NonSerialized] public bool SprintEvent, JumpEvent, Grounded, KeyEvent;
    [NonSerialized] public float VAxisEvent, HAxisEvent, HAxisRawEvent, RotateXEvent, Speed;
    [NonSerialized] public Checkpoint[] allCheckpoints;

    [SerializeField] private GameObject mainPlayer;
    [SerializeField] private short doorSensorDistance = 2;

    private Transform _transform;

    private short _checkpoint;
    private int _instanceId;


    // CAR GA

    [SerializeField] bool UseUserInput = false; // Defines whether the car uses a NeuralNetwork or user input
    [SerializeField] LayerMask SensorMask; // Defines the layer of the walls ("Wall")

    [SerializeField]
    float FitnessUnchangedDie = 5; // The number of seconds to wait before checking if the fitness didn't increase

    public static NeuralNetwork
        NextNetwork =
            new NeuralNetwork(new uint[] {8, 10, 5, 3},
                              null); // public NeuralNetwork that refers to the next neural network to be set to the next instantiated car

    public string TheGuid { get; private set; } // The Unique ID of the current car

    public int fitness;

    public int checkpointsMet;

    public AllCharactersInfo allCharacters;

    public NeuralNetwork TheNetwork { get; private set; } // The NeuralNetwork of the current car

    Rigidbody TheRigidbody; // The Rigidbody of the current car
    LineRenderer TheLineRenderer; // The LineRenderer of the current car


    private RayController _rayController;


    private float _angleFromCheckpoint;

    private double _distanceFromCheckpoint;


    public void MoveCharacter(bool pressKey = false,
                              bool sprint = false,
                              float rotate = 0,
                              float moveVertical = 0,
                              float moveHorizontal = 0)
    {
        KeyEvent = pressKey;
        SprintEvent = sprint;
        JumpEvent = false;
        RotateXEvent = rotate;
        VAxisEvent = moveVertical;
        HAxisEvent = moveHorizontal;
        if (moveHorizontal > 0)
            HAxisRawEvent = 1;
        else if (moveHorizontal < 0)
            HAxisRawEvent = -1;
        else
            HAxisRawEvent = 0;
    }

    void GetNeuralInputAxis(out float Vertical, out float Horizontal, out float Rotation) //, out float Rotation)
    {
        double[] NeuralInput = new double[NextNetwork.Topology[0]]; //TODO: lookat

        // Cast forward, back, right and left
        NeuralInput[0] = _rayController.north / 4;
        NeuralInput[1] = _rayController.south / 4;
        NeuralInput[2] = _rayController.east / 4;
        NeuralInput[3] = _rayController.west / 4;

        // Cast forward-right and forward-left
        NeuralInput[4] = _rayController.northeast / 4;
        NeuralInput[5] = _rayController.northwest / 4;


        NeuralInput[6] = _angleFromCheckpoint;
        NeuralInput[7] = _distanceFromCheckpoint;

        // Feed through the network
        double[] NeuralOutput = TheNetwork.FeedForward(NeuralInput);

        // Get Vertical Value
//        if (NeuralOutput[0] <= 0.25f)
//            Vertical = -1;
//        else if (NeuralOutput[0] >= 0.75f)
//            Vertical = 1;
//        else
//            Vertical = 0;
//
//        // Get Horizontal Value
//        if (NeuralOutput[1] <= 0.25f)
//            Horizontal = -1;
//        else if (NeuralOutput[1] >= 0.75f)
//            Horizontal = 1;
//        else
//            Horizontal = 0;

        // If the output is just standing still, then move the car forward
//        if (Vertical == 0 && Horizontal == 0)
//            Vertical = 1;

        Vertical = Mathf.RoundToInt((float) NeuralOutput[0]);
        Horizontal = Mathf.RoundToInt((float) NeuralOutput[1]);
        Rotation = (float) NeuralOutput[2];

        if (Mathf.RoundToInt((float) NeuralOutput[0]) > 1)
            print(NeuralOutput[0]);

        if (Mathf.RoundToInt((float) NeuralOutput[1]) > 1)
            print(NeuralOutput[1]);
    }

    IEnumerator IsNotImproving()
    {
        while (characterType == Type.GA_NPC)
        {
//            int OldFitness = fitness + 2 - Mathf.Abs(fitness) % 2; // Save the initial fitness
//            yield return new WaitForSeconds(FitnessUnchangedDie); // Wait for some time
//            if (OldFitness == fitness + 2 - Mathf.Abs(fitness) % 2) // Check if the fitness didn't change yet
//                WallHit(); // Kill this car
            yield return new WaitForSeconds(150);
            WallHit();
        }
    }

    public void CheckpointHit()
    {
        checkpointsMet++; // Increase Fitness/Score
        _checkpoint++;
    }

    public void WallHit()
    {
        EvolutionManager.Singleton.CarDead(allCharacters, fitness); // Tell the Evolution Manager that the car is dead
//        gameObject.SetActive(false); // Make sure the car is inactive
    }

    private void Start()
    {
        _transform = GetComponentInChildren<CharacterMoverController>().transform;
        _instanceId = transform.parent.gameObject.GetInstanceID();
        _rayController = _transform.GetComponentInChildren<RayController>();

        TheGuid = Guid.NewGuid().ToString(); // Assigns a new Unique ID for the current car

        TheNetwork = NextNetwork; // Sets the current network to the Next Network
        NextNetwork =
            new NeuralNetwork(NextNetwork.Topology,
                              null); // Make sure the Next Network is reassigned to avoid having another car use the same network

        TheRigidbody = GetComponentInChildren<Rigidbody>(); // Assign Rigidbody
        TheLineRenderer = GetComponentInChildren<LineRenderer>(); // Assign LineRenderer

        StartCoroutine(IsNotImproving()); // Start checking if the score stayed the same for a lot of time

        TheLineRenderer.positionCount = 17; // Make sure the line is long enough

        allCharacters = transform.parent.GetComponent<AllCharactersInfo>();
    }

    private void FixedUpdate()
    {
        switch (characterType)
        {
            case Type.NPC:
                MoveByCheckpoint();
                break;
            case Type.Player:
                MoveByPlayerInput();
                break;
            case Type.GA_NPC:
                MoveByGAInput();
                break;
            case Type.NN_NPC:
                MoveByNNInput();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Speed = SprintEvent ? sprintSpeed : runSpeed;
    }

    private void MoveByNNInput()
    {
        DataContainer.keyButtonPressed = KeyEvent;
        DataContainer.mouseRotation = RotateXEvent;
        DataContainer.forwardButtonPressed = VAxisEvent > 0;
        DataContainer.backButtonPressed = VAxisEvent < 0;
        DataContainer.leftButtonPressed = HAxisEvent < 0;
        DataContainer.rightButtonPressed = HAxisEvent > 0;

        if (allCheckpoints == null)
            return;

        if (_checkpoint < allCheckpoints.Length)
        {
            var checkpoint = allCheckpoints[_checkpoint];


            if (checkpoint.keyTextController.linkedKey.Contains(checkpoint.key.gameObject.GetInstanceID())
                && checkpoint.keyTextController.buttonPressed)
            {
                checkpoint.updateStatusOf(checkpoint.met, _instanceId, true);
                checkpoint.updateStatusOf(checkpoint.frontClosed, _instanceId, false);
                checkpoint.moveOn();
                _checkpoint++;
            }
        }
    }

    private void MoveByPlayerInput()
    {
        MoveCharacter(Input.GetKey(KeyCode.E),
                      Input.GetButton("Sprint"),
                      Input.GetAxis("Mouse X"),
                      Input.GetAxis("Vertical"),
                      Input.GetAxis("Horizontal"));

        DataContainer.keyButtonPressed = Input.GetKey(KeyCode.E);
        DataContainer.mouseRotation = Input.GetAxis("Mouse X");
        DataContainer.forwardButtonPressed = Input.GetAxisRaw("Vertical") > 0;
        DataContainer.backButtonPressed = Input.GetAxisRaw("Vertical") < 0;
        DataContainer.leftButtonPressed = Input.GetAxis("Horizontal") < 0;
        DataContainer.rightButtonPressed = Input.GetAxis("Horizontal") > 0;

        if (allCheckpoints == null)
            return;

        if (_checkpoint < allCheckpoints.Length)
        {
            var checkpoint = allCheckpoints[_checkpoint];


            if (checkpoint.keyTextController.linkedKey.Contains(checkpoint.key.gameObject.GetInstanceID())
                && checkpoint.keyTextController.buttonPressed)
            {
                checkpoint.updateStatusOf(checkpoint.met, _instanceId, true);
                checkpoint.updateStatusOf(checkpoint.frontClosed, _instanceId, false);
                checkpoint.moveOn();
                _checkpoint++;
            }
        }
    }

    private void MoveByGAInput()
    {
        if (allCheckpoints == null)
            return;

        if (_checkpoint < allCheckpoints.Length)
        {
            var checkpoint = allCheckpoints[_checkpoint];

            var checkpointDoor = checkpoint.frontDoor.transform;
            var checkpointDoorPosition = checkpointDoor.position;
            var checkpointDoorForward = checkpointDoor.forward;

            var width = (checkpoint.frontDoorCollider.bounds.size.y);

            float xAdjustment;
            float zAdjustment;

            if (checkpoint.frontDoor.transform.forward.z > 0.1)
            {
                xAdjustment = checkpoint.frontDoorCollider.bounds.size.y / 2;
                zAdjustment = 0;
            }
            else if (checkpoint.frontDoor.transform.forward.z < -0.1)
            {
                xAdjustment = -checkpoint.frontDoorCollider.bounds.size.y / 2;
                zAdjustment = 0;
            }
            else if (checkpoint.frontDoor.transform.forward.x > 0)
            {
                xAdjustment = 0;
                zAdjustment = -checkpoint.frontDoorCollider.bounds.size.y / 2;
            }
            else if (checkpoint.frontDoor.transform.forward.x < 0)
            {
                xAdjustment = 0;
                zAdjustment = checkpoint.frontDoorCollider.bounds.size.y / 2;
            }
            else
            {
                xAdjustment = 0;
                zAdjustment = 0;
            }

            var doorPosition = new Vector3(
                checkpointDoorPosition.x + width * checkpointDoorForward.x + xAdjustment,
                checkpointDoor.position.y,
                checkpointDoorPosition.z + width * checkpointDoorForward.z + zAdjustment); //TODO: door parent cleanup

            var distanceFromDoor = DistanceBetween(
                new Vector2(_transform.position.x, _transform.position.z),
                new Vector2(doorPosition.x, doorPosition.z));

            var distanceFromNpc = DistanceBetween(
                new Vector2(_transform.position.x, _transform.position.z),
                new Vector2(mainPlayer.transform.position.x, mainPlayer.transform.position.z));

            (_distanceFromCheckpoint, _angleFromCheckpoint) = GetCheckpoint(_checkpoint);

//            if (checkpoint.getStatusOf(checkpoint.frontClosed, _instanceId) &&
//                Mathf.RoundToInt((float) distanceFromDoor) <= 0)
//            {
//                MoveCharacter();
//                return;
//            }

//
//            if (_checkpoint >= allCheckpoints.Length)
//            {
//                MoveCharacter(false, false, 0, 0, 0);
//                return;
//            }
//            fitness = Mathf.RoundToInt(200 - (float) distanceFromDoor)
//                      + checkpointsMet * 1000
//                      + Mathf.RoundToInt(200 - (float) distanceFromNpc)
//                      + Mathf.RoundToInt(200 - (float) distanceFromCheckpoint);
            fitness = Mathf.RoundToInt(200 - (float) _distanceFromCheckpoint)
                      + _checkpoint * 200;
            if (checkpoint.keyTextController.linkedKey.Contains(checkpoint.key.gameObject.GetInstanceID())
                && checkpoint.keyTextController.buttonPressed) //TODO: Not working with asynchronous
            {
                checkpoint.updateStatusOf(checkpoint.met, _instanceId, true);
                checkpoint.updateStatusOf(checkpoint.frontClosed, _instanceId, false);
                checkpoint.moveOn();
                _checkpoint++;
            }

            DataContainer.fitness = _angleFromCheckpoint;
        }

        if (UseUserInput) // If we're gonna use user input
            MoveCharacter(Input.GetKey(KeyCode.E), //TODO: One pressing opens for another or not?
                          Input.GetButton("Sprint"),
                          Input.GetAxis("Mouse X"),
                          Input.GetAxis("Vertical"),
                          Input.GetAxis("Horizontal"));
        else // if we're gonna use a neural network
        {
            float Vertical;
            float Horizontal;
            float Rotation;

            GetNeuralInputAxis(out Vertical, out Horizontal, out Rotation);
            MoveCharacter(moveVertical: Vertical,
                          moveHorizontal: Horizontal,
                          rotate: Rotation);
        }
    }

    private void MoveByCheckpoint()
    {
        if (allCheckpoints == null)
            return;
        double distance;
        float normalisedAngle;
        var distanceFromPlayer = DistanceBetween(_transform.position, mainPlayer.transform.position);
        var angleFromPlayer = LookBetween(mainPlayer.transform, _transform);
        var awayFromPlayer = distanceFromPlayer > 10;
        
        if (_checkpoint > 0)
        {
            var checkpoint = allCheckpoints[_checkpoint - 1];
            var checkpointDoor = checkpoint.frontDoor.transform;
            var checkpointDoorPosition = checkpointDoor.position;
            var checkpointDoorForward = checkpointDoor.forward;

            var width = (checkpoint.frontDoorCollider.bounds.size.y + doorSensorDistance);

            float xAdjustment;
            float zAdjustment;

            if (checkpoint.frontDoor.transform.forward.z > 0.1)
            {
                xAdjustment = checkpoint.frontDoorCollider.bounds.size.y / 2;
                zAdjustment = 0;
            }
            else if (checkpoint.frontDoor.transform.forward.z < -0.1)
            {
                xAdjustment = -checkpoint.frontDoorCollider.bounds.size.y / 2;
                zAdjustment = 0;
            }
            else if (checkpoint.frontDoor.transform.forward.x > 0)
            {
                xAdjustment = 0;
                zAdjustment = -checkpoint.frontDoorCollider.bounds.size.y / 2;
            }
            else if (checkpoint.frontDoor.transform.forward.x < 0)
            {
                xAdjustment = 0;
                zAdjustment = checkpoint.frontDoorCollider.bounds.size.y / 2;
            }
            else
            {
                xAdjustment = 0;
                zAdjustment = 0;
            }

            var doorPosition = new Vector3(
                checkpointDoorPosition.x + width * checkpointDoorForward.x + xAdjustment,
                checkpointDoor.position.y,
                checkpointDoorPosition.z + width * checkpointDoorForward.z + zAdjustment); //TODO: door parent cleanup

            var distanceFromDoor = DistanceBetween(
                new Vector2(_transform.position.x, _transform.position.z),
                new Vector2(doorPosition.x, doorPosition.z));

            if (!checkpoint.getStatusOf(checkpoint.frontClosed, _instanceId)
                && Mathf.RoundToInt((float) distanceFromDoor) <= 1)
            {
                checkpoint.CloseFrontDoor(_instanceId);
            }

            DataContainer.doorClosed = checkpoint.getStatusOf(checkpoint.frontClosed, _instanceId);
            DataContainer.checkpointMet = !checkpoint.getStatusOf(checkpoint.met, _instanceId)
                                              ? _checkpoint - 1
                                              : _checkpoint;
        }

        else
        {
            DataContainer.doorClosed = false;
            DataContainer.checkpointMet = 0;
        }

        DataContainer.distanceFromPlayer = distanceFromPlayer;
        DataContainer.rotation = angleFromPlayer;
        if (_checkpoint >= allCheckpoints.Length || awayFromPlayer)
        {
            MoveCharacter();
            return;
        }

        (distance, normalisedAngle) = GetCheckpoint(_checkpoint);

        MoveCharacter(false,
                      false,
                      normalisedAngle * rotationSpeed,
                      distance < 1.3
                          ? IncrementCheckpoint()
                          : 1 - Mathf.Abs(normalisedAngle),
                      0); //TODO: simplify // 2 * normalisedAngle
    }

    private (double, float) GetCheckpoint(short checkpoint)
    {
        var target = allCheckpoints[checkpoint].flag.transform.position;
        var currentOffset = _transform.InverseTransformPoint(target);

        var angle = Quaternion.LookRotation(currentOffset).eulerAngles.y;
        return (DistanceBetween(_transform.position, target),
                Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    private short IncrementCheckpoint()
    {
//Destroy(checkpoints[_checkpoint].gameObject);
        _checkpoint++;
        return 0;
    }

    private float LookBetween(Transform source, Transform target)
    {
        var currentOffset = source.InverseTransformPoint(target.position);
        return Quaternion.LookRotation(currentOffset).eulerAngles.y;
    }


    private static double DistanceBetween(Vector3 v1, Vector3 v2)
    {
        var vector = new Vector3(v1.x - v2.x,
                                 v1.y - v2.y,
                                 v1.z - v2.z);
        return Math.Sqrt(vector.x * vector.x +
                         vector.y * vector.y +
                         vector.z * vector.z);
    }

    private static double DistanceBetween(Vector2 v1, Vector2 v2)
    {
        var vector = new Vector2(v1.x - v2.x,
                                 v1.y - v2.y);
        return Math.Sqrt(vector.x * vector.x +
                         vector.y * vector.y);
    }
}