﻿using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMoverController : MonoBehaviour
{
    [SerializeField] private GameObject minimapDot;
    
    private Rigidbody _body;
    private CharacterBehaviour _container;

    private void Start()
    {
        _container = gameObject.GetComponentInParent<CharacterBehaviour>();
        _body = gameObject.GetComponent<Rigidbody>();

        gameObject.layer = _container.gameObject.layer;
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.collider.CompareTag("Environment"))
            _container.Grounded = true;
    }

    private void FixedUpdate()
    {
        var jumping = _container.JumpEvent;
        var rotatingX = _container.RotateXEvent;
        var verticalAxis = _container.VAxisEvent;
        var horizontalAxis = _container.HAxisEvent;
        var transform = this.transform;

        if (jumping && _container.Grounded)
        {
            _body.AddForce(new Vector3(0, _container.jumpSpeed * _body.mass, 0),
                           ForceMode.Impulse);
            _container.Grounded = false;
        }

        transform.Rotate(_container.lookSpeed * new Vector3(0, rotatingX));

        transform.position += _container.Speed * Time.deltaTime
                                               * (horizontalAxis * transform.right
                                                  + verticalAxis * transform.forward);
    }
}