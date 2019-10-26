using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CharacterDummyController : CharacterBehaviour
{
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");

    [SerializeField] private GameObject sibling;

    private Rigidbody _siblingBody;
    private Animator _animator;

    private void Start()
    {
        _siblingBody = sibling.GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        var isJumping = Input.GetButton("Jump");
        _animator.SetBool(IsJumping, isJumping);
    }

    private void FixedUpdate()
    {
        var transform = this.transform;
        var verticalAxis = Input.GetAxis("Vertical");
        var horizontalAxis = Math.Abs(verticalAxis) > 0 ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal");

        transform.rotation = SmoothRotate(horizontalAxis, verticalAxis);
        transform.position = CopySiblingPosition(sibling.transform.position);


        if (Math.Abs(verticalAxis) > 0 || Math.Abs(horizontalAxis) > 0)
        {
            _animator.SetBool(IsRunning, true);
        }
        else
        {
            _animator.SetBool(IsRunning, false);
        }
    }

    private Quaternion SmoothRotate(float inputAxisX, float inputAxisY)
    {
        var inputVector = new Vector3(speed * inputAxisX,
                                      _siblingBody.velocity.y,
                                      speed * inputAxisY);
        var lookAtTarget = transform.position
                           + new Vector3(inputVector.x, 0, inputVector.z);
        var lookAtRotation = lookAtTarget - transform.position == Vector3.zero
                                 ? Quaternion.identity
                                 : Quaternion.LookRotation(lookAtTarget - transform.position);
        return Quaternion.Slerp(transform.rotation,
                                _siblingBody.rotation * lookAtRotation,
                                rotationSpeed / 100f);
    }

    private Vector3 CopySiblingPosition(Vector3 siblingPosition)
    {
        var relativeY = transform.TransformPoint(siblingPosition - transform.position)
                                 .y - 0.94f; //TODO: fix hack
        return new Vector3(siblingPosition.x, relativeY, siblingPosition.z);
    }
}