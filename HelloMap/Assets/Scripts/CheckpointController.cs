using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    [SerializeField] public Animator door;
    [SerializeField] private Animator[] backDoors;
    [SerializeField] private Collider key;

    [SerializeField] public Collider doorCollider;

    [NonSerialized] public bool Met;

    private static readonly int Open = Animator.StringToHash("Open");
    private static readonly int Close = Animator.StringToHash("Close");

    private GameObject _keyPrompt;
    private KeyTextController _keyTextController;


    private void Start()
    {
        _keyPrompt = GameObject.Find("KeyPrompt");
        _keyTextController = _keyPrompt.GetComponent<KeyTextController>();

        foreach (var backDoor in backDoors)
        {
            backDoor.SetTrigger(Open);
        }

        door.SetTrigger(Open);
    }

    private void Update()
    {
        if (ReferenceEquals(key, null)) // TODO: nullcheck
            return;
        if (_keyTextController.LinkedObject.Equals(key.gameObject.GetInstanceID())
            && Input.GetKey(KeyCode.E))
        {
            Met = true;
            door.SetTrigger(Open);
            foreach (var backDoor in backDoors)
            {
                backDoor.SetTrigger(Close);
            }
        }
    }

    public void CloseDoor()
    {
        door.SetTrigger(Close);
    }

//    private IEnumerator OnTriggerEnter(Collider other)
//    {
//        if (!other.CompareTag("NPC")) yield break;
//        yield return new WaitForSeconds(3);
//        door.SetTrigger(Close);
//    }
}