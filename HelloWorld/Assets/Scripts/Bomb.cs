using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bomb : MonoBehaviour
{
    private Rigidbody rigidbody;

    // Update is called once per frame
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        Respawn();
    }

    void Update()
    {
        if (transform.position.y < -10)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        var randomX = Random.Range(-10, 10);
        var randomY = Random.Range(10, 20);
        transform.position = new Vector3(randomX, randomY);
        rigidbody.velocity = Vector3.zero;
    }
}