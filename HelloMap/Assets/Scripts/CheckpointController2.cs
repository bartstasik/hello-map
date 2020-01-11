using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController2 : MonoBehaviour
{
    public Checkpoint[] checkpoints;

    private static readonly int Open = Animator.StringToHash("Open");

    public void LocateCheckpoints()
    {
        var allCheckpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        var checkpointsTemp = new List<Checkpoint>();

        foreach (var parentCheckpoint in allCheckpoints)
        {
            var cleanCheckpointChild = parentCheckpoint.AddComponent<Checkpoint>();
            foreach (var dirtyCheckpointChild in parentCheckpoint.GetComponentsInChildren<Transform>())
            {
                switch (dirtyCheckpointChild.name)
                {
                    case "Flag":
                        cleanCheckpointChild.flag = dirtyCheckpointChild.gameObject;
                        break;
                    case "Door_Front":
                        cleanCheckpointChild.frontDoorCollider =
                            dirtyCheckpointChild.GetComponentInChildren<Collider>();
                        cleanCheckpointChild.frontDoor = dirtyCheckpointChild.GetComponentInChildren<Animator>();
                        cleanCheckpointChild.frontDoor.SetTrigger(Open);
                        break;
                    case "Door_Back":
                        cleanCheckpointChild.backDoor = dirtyCheckpointChild.GetComponentInChildren<Animator>();
                        cleanCheckpointChild.backDoor.SetTrigger(Open);
                        break;
                    case "Key":
                        cleanCheckpointChild.key = dirtyCheckpointChild.GetComponent<Collider>();
                        break;
                }
            }

            checkpointsTemp.Add(cleanCheckpointChild);
        }

        checkpoints = checkpointsTemp.ToArray();
    }
}

public class Checkpoint : MonoBehaviour
{
    [NonSerialized] public GameObject flag;
    [NonSerialized] public Collider key, frontDoorCollider;
    [NonSerialized] public Animator frontDoor, backDoor;
    [NonSerialized] public bool frontClosed, met;

    private GameObject _keyPrompt;
    private KeyTextController _keyTextController;

    private static readonly int Open = Animator.StringToHash("Open");
    private static readonly int Close = Animator.StringToHash("Close");
    
    public void CloseFrontDoor()
    {
        frontDoor.SetTrigger(Close);
        frontClosed = true;
    }

    private void Start()
    {
        _keyPrompt = GameObject.Find("KeyPrompt");
        _keyTextController = _keyPrompt.GetComponent<KeyTextController>();
    }
    private void Update()
    {
        if (!_keyTextController.linkedObject.Equals(key.gameObject.GetInstanceID())
            || !Input.GetKey(KeyCode.E)) //TODO: Fix hack
            return;
        met = true;
        frontClosed = true;
        frontDoor.SetTrigger(Open);
        backDoor?.SetTrigger(Close);
    }
}