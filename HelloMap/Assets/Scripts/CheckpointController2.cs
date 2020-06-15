using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MultilevelController;

public class CheckpointController2 : MonoBehaviour
{
    public Checkpoint[] checkpoints;

    private static readonly int Open = Animator.StringToHash("Open");

    public void LocateCheckpoints(GamePlayMode playMode = GamePlayMode.Normal)
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

            cleanCheckpointChild.playMode = playMode;
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
    [NonSerialized] public Dictionary<int, bool> frontClosed, met;
    [NonSerialized] public KeyTextController keyTextController;
    [NonSerialized] public GamePlayMode playMode;

    private static readonly int Open = Animator.StringToHash("Open");
    private static readonly int Close = Animator.StringToHash("Close");

    public void CloseFrontDoor(int instanceId)
    {
        frontDoor.SetTrigger(Close);
        updateStatusOf(frontClosed, instanceId, true);
    }

    public void updateStatusOf(Dictionary<int, bool> variable,
                               int instanceId,
                               bool status)
    {
        if (!variable.ContainsKey(instanceId))
            variable.Add(instanceId, status);
        else
            variable[instanceId] = status;
    }

    public bool getStatusOf(Dictionary<int, bool> variable,
                            int instanceId)
    {
        return variable.ContainsKey(instanceId) && variable[instanceId];
    }

    public void moveOn()
    {
        frontDoor.SetTrigger(Open);
        backDoor?.SetTrigger(Close);
    }

    private void Start()
    {
        keyTextController = GameObject.Find("KeyPrompt").GetComponent<KeyTextController>();
        met = new Dictionary<int, bool>();
        frontClosed = new Dictionary<int, bool>();
        flag.SetActive(true);
    }

    private void Update()
    {
        if (playMode != GamePlayMode.Normal && playMode != GamePlayMode.NN_REC
            || !keyTextController.buttonPressed //TODO: Not working with asynchronous
            || !keyTextController.linkedKey.Contains(key.gameObject.GetInstanceID()))
            return;
//        updateStatusOf(met, instanceId, true);
//        updateStatusOf(frontClosed, instanceId, true);
        frontDoor.SetTrigger(Open);
        backDoor?.SetTrigger(Close);
    }
}