using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyTextController : MonoBehaviour
{
    [SerializeField] private Text seeKeyText;
    
    [NonSerialized] public int linkedObject = -1;

    private GameObject _promptCanvas;

    private void Start()
    {
        _promptCanvas = GameObject.Find("PressE").gameObject;
    }

    private void Update()
    {
        _promptCanvas.SetActive(linkedObject != -1);
        seeKeyText.text = "Key Seen : " + (linkedObject != -1);
    }
}