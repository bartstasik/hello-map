using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyTag : MonoBehaviour
{
    [SerializeField] private bool stayActive = true;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(stayActive);
    }
}
