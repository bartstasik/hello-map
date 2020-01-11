using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroButtonController : MonoBehaviour
{
    private Button _playButton;

    private void FirstLevel()
    {
        SceneManager.LoadScene(SceneManager.sceneCount);
    }

    private void Start()
    {
        _playButton = GetComponent<Button>();
        _playButton.onClick.AddListener(FirstLevel);
    }
}
