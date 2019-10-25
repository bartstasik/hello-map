using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class CubeMover : MonoBehaviour
{
    
    [SerializeField]
    public GameObject myPrefab;
    
    [SerializeField]
    private float speed = 1;

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, vertical);

        transform.position += speed * Time.deltaTime * movement;
    }

    private void OnCollisionEnter(Collision other)
    {
        Instantiate(myPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }
}