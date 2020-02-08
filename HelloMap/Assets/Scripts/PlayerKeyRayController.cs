using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerKeyRayController : MonoBehaviour
{
    private GameObject _text;
    private KeyTextController _textController;
    private CharacterBehaviour _container;
    private int lastInstanceId;

    void Start()
    {
        _text = GameObject.Find("KeyPrompt").gameObject;
        _textController = _text.GetComponent<KeyTextController>();
        _container = gameObject.GetComponentInParent<CharacterBehaviour>();
    }

    void Update()
    {
        var collider = CastRay();
        var keyLayer = LayerMask.NameToLayer("Key");
        var instanceId = ReferenceEquals(collider, null) ? -1 : collider.gameObject.GetInstanceID();
        if (!ReferenceEquals(collider, null) && collider.gameObject.layer == keyLayer)
        {
            _textController.linkedKey.Add(instanceId);
            lastInstanceId = instanceId;
        }
        else if (lastInstanceId != -1)
        {
            _textController.linkedKey.Remove(lastInstanceId); //TODO check if collision -1
            lastInstanceId = -1;
        }
        _textController.buttonPressed = _container.KeyEvent; // TODO: change when asynchronous
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