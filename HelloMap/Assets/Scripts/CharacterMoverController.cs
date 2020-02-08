using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMoverController : MonoBehaviour
{
    private Rigidbody _body;
    private CharacterBehaviour _container;

    private void Start()
    {
        _container = gameObject.GetComponentInParent<CharacterBehaviour>();
        _body = gameObject.GetComponent<Rigidbody>();

        gameObject.tag = _container.characterType.ToString();
        gameObject.layer = LayerMask.NameToLayer(_container.characterType.ToString());
        Physics.IgnoreLayerCollision(gameObject.layer, gameObject.layer);

        switch (_container.characterType)
        {
            case CharacterBehaviour.Type.NPC:
                Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Door"));
                break;
            case CharacterBehaviour.Type.GA_NPC:
                Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Door"));
                Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("NPC"));
                break;
            case CharacterBehaviour.Type.Player:
                Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("NPC"));
                break;
            case CharacterBehaviour.Type.NN_NPC:
                Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("NPC"));
                break;
            default:
                throw new Exception("Invalid character type");
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.collider.CompareTag("Environment"))
            _container.Grounded = true;
    }

//    private void OnCollisionEnter(Collision collision) // Once anything hits the wall
//    {
//        if (_container.characterType == CharacterBehaviour.Type.GA_NPC
//            && collision.gameObject.layer == LayerMask.NameToLayer("Environment")) // Make sure it's a car
//        {
//            var componentInParent = GetComponentInParent<CharacterBehaviour>();
//            componentInParent.WallHit(); // If it is a car, tell it that it just hit a wall
//        }
//    }

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