﻿using System;
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
        NPC
    }

    public Type characterType = Type.NPC;

    public float runSpeed = 5;
    public float sprintSpeed = 10;
    public float jumpSpeed = 3;
    public float lookSpeed = 5;
    public float rotationSpeed = 10;
    
    [NonSerialized] public bool SprintEvent, JumpEvent, Grounded;
    [NonSerialized] public float VAxisEvent, HAxisEvent, HAxisRawEvent, RotateXEvent, Speed;
    [NonSerialized] public Checkpoint[] allCheckpoints;
    
    [SerializeField] private GameObject mainPlayer;
    [SerializeField] private Text doorClosedText, checkpointMetText, awayPlayerText;

    [SerializeField] private short doorSensorDistance = 2;

    private Transform _transform;

    private short _checkpoint;

    public void MoveCharacter(bool spring,
                              float rotate,
                              float moveVertical,
                              float moveHorizontal)
    {
        SprintEvent = spring;
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
            default:
                throw new ArgumentOutOfRangeException();
        }

        Speed = SprintEvent ? sprintSpeed : runSpeed;
    }

    private void MoveByPlayerInput()
    {
        MoveCharacter(Input.GetButton("Sprint"),
                      Input.GetAxis("Mouse X"),
                      Input.GetAxis("Vertical"),
                      Input.GetAxis("Horizontal"));
    }

    private void MoveByCheckpoint()
    {
        if (allCheckpoints == null)
            return;
        
        double distance;
        float normalisedAngle;

        var distanceFromPlayer = DistanceBetween(_transform.position, mainPlayer.transform.position);
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

            if (!checkpoint.frontClosed && Mathf.RoundToInt((float) distanceFromDoor) <= 1)
            {
                checkpoint.CloseFrontDoor();
            }

            doorClosedText.text = "Door Closed : " + checkpoint.frontClosed;
            checkpointMetText.text = "Checkpoint Met : " + (!checkpoint.met ? _checkpoint - 1 : _checkpoint);
        }
        else
        {
            doorClosedText.text = "Door Closed : " + false;
            checkpointMetText.text = "Checkpoint Met : " + 0;
        }

        awayPlayerText.text = "Away from Player : " + distanceFromPlayer;

        if (_checkpoint >= allCheckpoints.Length || awayFromPlayer)
        {
            MoveCharacter(false, 0, 0, 0);
            return;
        }

        (distance, normalisedAngle) = GetCheckpoint(_checkpoint);

        MoveCharacter(false,
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