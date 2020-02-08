using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyTextController : MonoBehaviour
{
    [NonSerialized] public HashSet<int> linkedKey;
    [NonSerialized] public bool buttonPressed;

    private GameObject _promptCanvas;

    private void Start()
    {
        _promptCanvas = GameObject.Find("PressE").gameObject;
        linkedKey = new HashSet<int>();
    }

    private void Update()
    {
        _promptCanvas.SetActive(linkedKey.Count != 0);
        DataContainer.seesKey = linkedKey.Count != 0;
    }
}