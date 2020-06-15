using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CharacterBehaviour.Type;

public class MinimapCameraController : MonoBehaviour
{
    [SerializeField] private GameObject dot;
    [SerializeField] private Material red, green, purple;

    private Dictionary<CharacterMoverController, GameObject> characters =
        new Dictionary<CharacterMoverController, GameObject>();

    public void AddCharacter(CharacterMoverController character,
                             CharacterBehaviour characterBehaviour)
    {
        var characterDot = Instantiate(dot, character.transform.position, Quaternion.identity);
        Material dotColour;
        switch (characterBehaviour.characterType)
        {
            case Player:
                dotColour = green;
                break;
            case NPC:
                dotColour = red;
                break;
            default:
                dotColour = purple;
                break;
        }

        characterDot.GetComponent<Renderer>().material = dotColour;
        characterDot.transform.parent = character.transform;
        characterDot.gameObject.SetActive(true);
        characters.Add(character, characterDot);
    }

    public void RemoveCharacter(CharacterMoverController character)
    {
        characters.Remove(character);
    }

    private void Update()
    {
        if (characters.Count <= 0)
            return;

        var fittest = characters.OrderByDescending(c => c.Key.GetComponentInParent<CharacterBehaviour>().fitness)
                                .First()
                                .Key;
        transform.position = fittest.transform.position;

        foreach (var character in characters)
        {
//            if (character.Key.CompareTag("Player") || character.Key.CompareTag("GA_NPC"))
//                transform.position = character.Key.transform.position;
            character.Value.transform.position = character.Key.transform.position;
        }
    }
}