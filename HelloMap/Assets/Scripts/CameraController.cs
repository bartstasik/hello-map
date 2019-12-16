using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CameraController : MonoBehaviour
{
    private float rotationSpeed;
    [SerializeField] GameObject character;

    private void Start()
    {
        rotationSpeed = GetComponentInParent<CharacterBehaviour>().lookSpeed;
//        character = GetComponentInParent<GameObject>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var difference = -Input.GetAxis("Mouse Y");
        if (transform.position.y > 2.35 && Input.GetAxis("Mouse Y") < 0) return;
        if (transform.position.y < 1 && Input.GetAxis("Mouse Y") > 0) return;
        if (Input.GetAxis("Mouse Y") == 0) return;

        var vector3 = new Vector3(transform.position.x,
                                  transform.position.y + difference,
                                  transform.position.z + difference);
        transform.LookAt(character.transform);
        transform.position = vector3;
    }
}