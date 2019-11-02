using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{

    [SerializeField] private Animator door;
    
    private static readonly int Open = Animator.StringToHash("Open");

    private void OnTriggerEnter(Collider other)
    {
        door.SetTrigger(Open);
    }
}