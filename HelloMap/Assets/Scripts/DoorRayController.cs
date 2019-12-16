using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRayController : MonoBehaviour
{
    private Animator _doorAnimator;
    private Collider _doorCollider;
    private static readonly int Close = Animator.StringToHash("Close");

    private void Start()
    {
        _doorAnimator = GetComponentInChildren<Animator>();
        _doorCollider = GetComponentInChildren<Collider>();
    }

    void Update()
    {
        if (CastRay())
            _doorAnimator.SetTrigger(Close);
    }

    private bool CastRay()
    {
        RaycastHit hit;
        var position = new Vector3(transform.position.x,
                                   transform.position.y + 15,
                                   transform.position.z);
        var direction = new Vector3(transform.forward.x,
                                    transform.forward.y + 15,
                                    transform.forward.z); //TODO: correct location of ray
        Physics.Raycast(position,
                        direction,
                        out hit,
                        Mathf.Infinity,
                        LayerMask.GetMask("NPC"));
        Debug.DrawRay(position, direction);
        return !ReferenceEquals(hit.collider, null); //TODO: expensive null check
    }
}