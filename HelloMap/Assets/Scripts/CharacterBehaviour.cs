using System;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class CharacterBehaviour : MonoBehaviour
{
    public enum Type
    {
        Player,
        NonPlayerCharacter
    }

    public Type playerType = Type.NonPlayerCharacter;

    public float runSpeed = 5;
    public float sprintSpeed = 10;
    public float jumpSpeed = 3;
    public float lookSpeed = 5;
    public float rotationSpeed = 10;

    [NonSerialized] public bool SprintEvent, JumpEvent, Grounded;
    [NonSerialized] public float VAxisEvent, HAxisEvent, HAxisRawEvent, RotateXEvent, Speed;

    [SerializeField] private GameObject mainPlayer;
    [SerializeField] private Text doorClosedText, checkpointMetText, awayPlayerText;

    [SerializeField] private short doorSensorDistance = 2;

    private Transform _transform;

    private GameObject[] _allCheckpoints;
    private CheckpointController[] _allCheckpointTriggers;

    private short _checkpoint;

    private void Start()
    {
        _allCheckpoints = GetComponentInParent<GlobalCharacter>().checkpoints;
        _allCheckpointTriggers = new CheckpointController[_allCheckpoints.Length];
        _transform = GetComponentInChildren<CharacterMoverController>().transform;

        for (var i = 0; i < _allCheckpointTriggers.Length; i++)
        {
            _allCheckpointTriggers[i] = _allCheckpoints[i].GetComponent<CheckpointController>();
        }
    }

    private void FixedUpdate()
    {
        switch (playerType)
        {
            case Type.NonPlayerCharacter:
                MoveByCheckpoint();
                break;
            case Type.Player:
                MoveByPlayerInput();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Speed = SprintEvent ? sprintSpeed : runSpeed;
    }

    private void MoveByPlayerInput()
    {
        SprintEvent = Input.GetButton("Sprint");
        JumpEvent = false; //Input.GetButton("Jump");
        RotateXEvent = Input.GetAxis("Mouse X");
        VAxisEvent = Input.GetAxis("Vertical");
        HAxisEvent = Input.GetAxis("Horizontal");
        HAxisRawEvent = Input.GetAxisRaw("Horizontal");
    }

    private void MoveByCheckpoint()
    {
        double distance;
        float normalisedAngle;

        var distanceFromPlayer = DistanceBetween(_transform.position, mainPlayer.transform.position);
        var awayFromPlayer = distanceFromPlayer > 10;

        if (_checkpoint > 0)
        {
            var checkpoint = _allCheckpointTriggers[_checkpoint - 1];
            var checkpointDoor = checkpoint.door.transform;
            var checkpointDoorPosition = checkpointDoor.position;
            var checkpointDoorForward = checkpointDoor.forward;

            var width = (checkpoint.doorCollider.bounds.size.y + doorSensorDistance);

            float xAdjustment;
            float zAdjustment;

            if (checkpoint.door.transform.forward.z > 0.1)
            {
                xAdjustment = checkpoint.doorCollider.bounds.size.y / 2;
                zAdjustment = 0;
            }
            else if (checkpoint.door.transform.forward.z < -0.1)
            {
                xAdjustment = -checkpoint.doorCollider.bounds.size.y / 2;
                zAdjustment = 0;
            }
            else if (checkpoint.door.transform.forward.x > 0)
            {
                xAdjustment = 0;
                zAdjustment = -checkpoint.doorCollider.bounds.size.y / 2;
            }
            else if (checkpoint.door.transform.forward.x < 0)
            {
                xAdjustment = 0;
                zAdjustment = checkpoint.doorCollider.bounds.size.y / 2;
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

            if (!checkpoint.closed && Mathf.RoundToInt((float) distanceFromDoor) <= 1)
            {
                checkpoint.CloseDoor();
            }

            doorClosedText.text = "Door Closed : " + checkpoint.closed;
            checkpointMetText.text = "Checkpoint Met : " + (!checkpoint.met ? _checkpoint - 1 : _checkpoint);
        }
        else
        {
            doorClosedText.text = "Door Closed : " + false;
            checkpointMetText.text = "Checkpoint Met : " + 0;
        }

        awayPlayerText.text = "Away from Player : " + distanceFromPlayer;

        if (_checkpoint >= _allCheckpoints.Length || awayFromPlayer)
        {
            RotateXEvent = 0;
            VAxisEvent = 0;
            return;
        }

        (distance, normalisedAngle) = GetCheckpoint(_checkpoint);

        SprintEvent = false;
        JumpEvent = false;
        RotateXEvent = normalisedAngle * rotationSpeed;
        VAxisEvent = distance < 1.3
                         ? IncrementCheckpoint()
                         : 1 - Mathf.Abs(normalisedAngle); //TODO: simplify // 2 * normalisedAngle
        HAxisEvent = 0;
        HAxisRawEvent = 0;
    }

    private (double, float) GetCheckpoint(short checkpoint)
    {
        var target = _allCheckpoints[checkpoint].transform.position;
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