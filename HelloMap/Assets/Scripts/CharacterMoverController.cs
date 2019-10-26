using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMoverController : CharacterBehaviour
{
    private Rigidbody _body;

    private void Start()
    {
        _body = gameObject.GetComponent<Rigidbody>();
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.collider.CompareTag("Environment"))
            grounded = true;
    }

    private void FixedUpdate()
    {
        var jumping = Input.GetButton("Jump");
        var rotatingX = Input.GetAxis("Mouse X");
        var verticalAxis = Input.GetAxis("Vertical");
        var horizontalAxis = Input.GetAxis("Horizontal");
        var transform = this.transform;

        if (jumping && grounded)
        {
            _body.AddForce(new Vector3(0, jumpSpeed * _body.mass, 0),
                           ForceMode.Impulse);
            grounded = false;
        }

        transform.Rotate(lookSpeed * new Vector3(0, rotatingX));

        transform.position += speed * Time.deltaTime
                                    * (horizontalAxis * transform.right
                                       + verticalAxis * transform.forward);
    }
}