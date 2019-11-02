using System;
using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    public enum Type
    {
        Player,
        NonPlayerCharacter
    }

    public Type playerType = Type.NonPlayerCharacter;

    public GameObject[] checkpoints;

    public float runSpeed = 5;
    public float sprintSpeed = 2;
    public float jumpSpeed = 3;
    public float lookSpeed = 5;
    public float rotationSpeed = 10;

    [NonSerialized] public bool SprintEvent, JumpEvent, Grounded;
    [NonSerialized] public float VAxisEvent, HAxisEvent, HAxisRawEvent, RotateXEvent, Speed;

    private short _checkpoint = 0;

    private Transform _transform;

    private void Start()
    {
        _transform = GetComponentInChildren<CharacterMoverController>().transform;
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

        Speed = SprintEvent ? runSpeed * sprintSpeed : runSpeed;
    }

    private void MoveByCheckpoint()
    {
        if (_checkpoint >= checkpoints.Length)
        {
            return;
        }

        var (distance, normalisedAngle) = GetCheckpoint();
        
        SprintEvent = false;
        JumpEvent = false;
        RotateXEvent = normalisedAngle * rotationSpeed;;
        VAxisEvent = distance < 1.3
                         ? IncrementCheckpoint()
                         : 1 - Mathf.Abs(normalisedAngle); //TODO: simplify // 2 * normalisedAngle
        HAxisEvent = 0;
        HAxisRawEvent = 0;
        
        print(1 - Mathf.Abs(normalisedAngle));
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

    private (double, float) GetCheckpoint()
    {
        var source = _transform.position;
        var target = checkpoints[_checkpoint].transform.position;
        var currentOffset = _transform.InverseTransformPoint(target);
        var angle = Quaternion.LookRotation(currentOffset).eulerAngles.y;
        var vector = new Vector3(source.x - target.x,
                                 source.y - target.y,
                                 source.z - target.z);
        var distance = Math.Sqrt(vector.x * vector.x +
                                 vector.y * vector.y +
                                 vector.z * vector.z);
        var normalisedAngle = Mathf.Sin(angle * Mathf.Deg2Rad);
        return (distance, normalisedAngle);
    }
    
    private short IncrementCheckpoint()
    {
        Destroy(checkpoints[_checkpoint]);
        _checkpoint++;
        return 0;
    }
}