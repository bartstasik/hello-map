using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTextController : MonoBehaviour
{
    public int LinkedObject = -1;

    private GameObject _prompCanvas;

    private void Start()
    {
        _prompCanvas = GameObject.Find("PressE").gameObject;
    }

    private void Update()
    {
        _prompCanvas.SetActive(LinkedObject != -1);
    }
}