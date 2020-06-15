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
        NN_NPC
    }

    public Type characterType = Type.NPC;

    public float runSpeed = 5;
    public float sprintSpeed = 10;
    public float jumpSpeed = 3;
    public float lookSpeed = 5;
    public float rotationSpeed = 10;

    public int fitness;
    public int checkpointsMet;

    public AllCharactersInfo allCharacters;

    [NonSerialized] public bool SprintEvent, JumpEvent, Grounded, KeyEvent;
    [NonSerialized] public float VAxisEvent, HAxisEvent, HAxisRawEvent, RotateXEvent, Speed;
    [NonSerialized] public Checkpoint[] allCheckpoints;

    [SerializeField] private GameObject mainPlayer;
    [SerializeField] private short doorSensorDistance = 2;

    private Transform _transform;

    private short _checkpoint;
    private int _instanceId;

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

    private void Start()
    {
        _transform = GetComponentInChildren<CharacterMoverController>().transform;
        _instanceId = transform.parent.gameObject.GetInstanceID();
        _rayController = _transform.GetComponentInChildren<RayController>();

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