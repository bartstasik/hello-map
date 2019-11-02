using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CharacterDummyController : MonoBehaviour
{
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");

    [SerializeField] private GameObject sibling;

    private CharacterBehaviour _container;
    private Rigidbody _siblingBody;
    private Animator _animator;

    private void Start()
    {
        _container = gameObject.GetComponentInParent<CharacterBehaviour>();
        _siblingBody = sibling.GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        _animator.SetBool(IsJumping, _container.JumpEvent);
    }

    private void FixedUpdate()
    {
        var transform = this.transform;
        var sprinting = _container.SprintEvent;
        var verticalAxis = _container.VAxisEvent;
        var horizontalAxis = Math.Abs(verticalAxis) > 0 ? _container.HAxisRawEvent : _container.HAxisEvent;

        transform.rotation = SmoothRotate(horizontalAxis, verticalAxis);
        transform.position = CopySiblingPosition(sibling.transform.position);


        if (Math.Abs(verticalAxis) > 0 || Math.Abs(horizontalAxis) > 0)
        {
            _animator.SetBool(IsRunning, true);
            if (sprinting)
            {
                _animator.SetBool(IsSprinting, true);
            }
            else
            {
                _animator.SetBool(IsSprinting, false);
            }
        }
        else
        {
            _animator.SetBool(IsRunning, false);
            _animator.SetBool(IsSprinting, false);
        }
    }

    private Quaternion SmoothRotate(float inputAxisX, float inputAxisY)
    {
        var inputVector = new Vector3(_container.Speed * inputAxisX,
                                      _siblingBody.velocity.y,
                                      _container.Speed * inputAxisY);
        var lookAtTarget = transform.position
                           + new Vector3(inputVector.x, 0, inputVector.z);
        var lookAtRotation = lookAtTarget - transform.position == Vector3.zero
                                 ? Quaternion.identity
                                 : Quaternion.LookRotation(lookAtTarget - transform.position);
        return Quaternion.Slerp(transform.rotation,
                                _siblingBody.rotation * lookAtRotation,
                                _container.rotationSpeed / 100f);
    }

    private Vector3 CopySiblingPosition(Vector3 siblingPosition)
    {
        var relativeY = transform.TransformPoint(siblingPosition - transform.position)
                                 .y - 0.94f; //TODO: fix hack
        return new Vector3(siblingPosition.x, relativeY, siblingPosition.z);
    }
}