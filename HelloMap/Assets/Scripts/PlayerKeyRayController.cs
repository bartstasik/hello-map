using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyRayController : MonoBehaviour
{
    private GameObject _text;
    private KeyTextController _textController;

    void Start()
    {
        _text = GameObject.Find("KeyPrompt").gameObject;
        _textController = _text.GetComponent<KeyTextController>();
    }

    void Update()
    {
        var collider = CastRay();
        var keyLayer = LayerMask.NameToLayer("Key");
        _textController.LinkedObject = !ReferenceEquals(collider, null) 
                                       && collider.gameObject.layer == keyLayer
                                           ? collider.gameObject.GetInstanceID()
                                           : -1; //TODO check if collision -1
    }

    private Collider CastRay()
    {
        RaycastHit hit;
        var position = transform.position + transform.up * 0.9f;
        var direction = transform.forward;

        var keyLayer = 1 << LayerMask.NameToLayer("Key");
        var envLayer = 1 << LayerMask.NameToLayer("Environment");
        var mask = keyLayer | envLayer;
        
        Physics.Raycast(position, //TODO: boxcast/spherecast or similar
                        direction,
                        out hit,
                        5,
                        mask);
        Debug.DrawRay(position, direction);
        return hit.collider; //TODO: expensive null check
    }
}