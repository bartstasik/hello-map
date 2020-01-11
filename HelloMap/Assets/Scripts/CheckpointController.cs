using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointController : MonoBehaviour
{
    [SerializeField] private Animator[] backDoors;
    [SerializeField] private Collider key;
    [SerializeField] private bool isLast;

    [SerializeField] public Animator door;
    
    [NonSerialized] public Collider doorCollider;
    [NonSerialized] public bool closed;
    [NonSerialized] public bool met;

    private static readonly int Open = Animator.StringToHash("Open");
    private static readonly int Close = Animator.StringToHash("Close");

    private GameObject _keyPrompt;
    private KeyTextController _keyTextController;


    private void Start()
    {
        _keyPrompt = GameObject.Find("KeyPrompt");
        _keyTextController = _keyPrompt.GetComponent<KeyTextController>();
        doorCollider = door.GetComponent<Collider>();

//        foreach (var backDoor in backDoors)
//            backDoor.SetTrigger(Open);
//
//        door.SetTrigger(Open);
    }

    private void Update()
    {
        if (!_keyTextController.linkedObject.Equals(key.gameObject.GetInstanceID())
            || !Input.GetKey(KeyCode.E))
            return;
        if (isLast)
        {
            LevelController.NextLevel();
            return;
        }
        met = true;
        closed = false;
        door.SetTrigger(Open);
        foreach (var backDoor in backDoors)
            backDoor.SetTrigger(Close);
    }

    public void CloseDoor()
    {
        door.SetTrigger(Close);
        closed = true;
    }
}