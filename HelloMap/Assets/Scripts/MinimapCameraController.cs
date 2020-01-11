using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CharacterBehaviour.Type;

public class MinimapCameraController : MonoBehaviour
{
    [SerializeField] private GameObject dot;
    [SerializeField] private Material red;
    [SerializeField] private Material green;

    private Dictionary<CharacterMoverController, GameObject> characters = new Dictionary<CharacterMoverController, GameObject>();

    public void AddCharacter(CharacterMoverController character)
    {
        var characterDot = Instantiate(dot, character.transform.position, Quaternion.identity);
        Material dotColour;
        switch (character.GetComponentInParent<CharacterBehaviour>().characterType)
        {
            case Player:
                dotColour = green;
                break;
            case NPC:
                dotColour = red;
                break;
            default:
                dotColour = red;
                break;
        }
        characterDot.GetComponent<Renderer>().material = dotColour;
        characterDot.transform.parent = character.transform;
        characterDot.SetActive(true);
        characters.Add(character, characterDot);
    }
    
    private void Update()
    {
        if (characters.Count <= 0)
            return;
        foreach (var character in characters)
        {
            if (character.Key.CompareTag("Player"))
                transform.position = character.Key.transform.position;
            character.Value.transform.position = character.Key.transform.position;
        }
    }
}