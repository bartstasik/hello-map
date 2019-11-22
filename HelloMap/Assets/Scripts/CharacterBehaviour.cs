using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterBehaviour : MonoBehaviour
{
    public enum Type
    {
        Player,
        NonPlayerCharacter
    }

    public Type playerType = Type.NonPlayerCharacter;
    
    public float runSpeed = 5;
    public float sprintSpeed = 2;
    public float jumpSpeed = 3;
    public float lookSpeed = 5;
    public float rotationSpeed = 10;

    [NonSerialized] public bool SprintEvent, JumpEvent, Grounded;
    [NonSerialized] public float VAxisEvent, HAxisEvent, HAxisRawEvent, RotateXEvent, Speed;

    [SerializeField] private GameObject mainPlayer;
    [SerializeField] private GameObject NPC;
    [SerializeField] private Text northwestText;
    [SerializeField] private Text southwestText;

    private Transform _transform;
    
    private GameObject[] _allCheckpoints;

    private GlobalCharacter _globalCharacter;

    private void Start()
    {
        _globalCharacter = GetComponentInParent<GlobalCharacter>();
        _allCheckpoints = _globalCharacter.checkpoints;
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

    private void MoveByPlayerInput()
    {
        SprintEvent = Input.GetButton("Sprint");
        JumpEvent = false; //Input.GetButton("Jump");
        RotateXEvent = Input.GetAxis("Mouse X");
        VAxisEvent = Input.GetAxis("Vertical");
        HAxisEvent = Input.GetAxis("Horizontal");
        HAxisRawEvent = Input.GetAxisRaw("Horizontal");
        
        if (_globalCharacter.playerCheckpoint >= _allCheckpoints.Length)
            return;
        
        var (distance, normalisedAngle) = GetCheckpoint(_globalCharacter.playerCheckpoint);

        print(_allCheckpoints[3]);
        
        if (distance < 1.3)
        {
            _globalCharacter.playerCheckpoint++;
//            _globalCharacter.npcCheckpoint++;
//            Destroy(_allCheckpoints[_checkpoint].gameObject);
        }

        southwestText.text = distance.ToString();
        northwestText.text = DistanceAlongPath().ToString();
    }

    private void MoveByCheckpoint()
    {
        double distance;
        float normalisedAngle;
        bool playerBehind, aheadByCheckpoint;

        if (_globalCharacter.npcCheckpoint == 0)
        {
            distance = 0;
        }
        else
        {
            distance = DistanceBetween(
                _transform.position,
                _allCheckpoints[_globalCharacter.npcCheckpoint - 1].transform.position);
        }

        playerBehind = _globalCharacter.npcCheckpoint > _globalCharacter.playerCheckpoint;
        aheadByCheckpoint = playerBehind && distance > 10;

        if (_globalCharacter.npcCheckpoint >= _allCheckpoints.Length || aheadByCheckpoint)
        {
            RotateXEvent = 0;
            VAxisEvent = 0;
            return;
        }
        
        (distance, normalisedAngle) = GetCheckpoint(_globalCharacter.npcCheckpoint);
        
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
        _globalCharacter.npcCheckpoint++;
        return 0;
    }

    private double DistanceAlongPath()
    {
        var a = 0d;
        var max = _allCheckpoints.Length;
        var player = _globalCharacter.playerCheckpoint;
        var npc = _globalCharacter.npcCheckpoint;
        
        if (player == npc || player == max - 1 && npc == max)
        {
            a = DistanceBetween(_transform.position, NPC.transform.position);
        } else if (player == npc - 1)
        {
            var target = _allCheckpoints[player].transform.position;
            a = DistanceBetween(_transform.position, target) 
                + DistanceBetween(target, NPC.transform.position);
        }

        return a;
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
}