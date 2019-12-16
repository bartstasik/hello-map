using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{

    [SerializeField] private GameObject dot;
    [SerializeField] private Material red;
    [SerializeField] private Material green;

    private Dictionary<GameObject, GameObject> characters = new Dictionary<GameObject, GameObject>();

    private void Start()
    {
        var playerCharacter = GameObject.FindGameObjectWithTag("Player");
        var npcCharacters = GameObject.FindGameObjectsWithTag("NPC");

        var playerDot = Instantiate(dot, playerCharacter.transform.position, Quaternion.identity);
        playerDot.GetComponent<Renderer>().material = green;
        playerDot.transform.parent = playerCharacter.transform;
        playerDot.SetActive(true);

        characters.Add(playerCharacter, playerDot);
        
        foreach (var npc in npcCharacters)
        {
            var npcDot = Instantiate(dot, npc.transform.position, Quaternion.identity);
            npcDot.GetComponent<Renderer>().material = red;
            npcDot.transform.parent = npc.transform;
            npcDot.SetActive(true);
            characters.Add(npc, npcDot);
        }
    }

    void FixedUpdate()
    {
        foreach (var character in characters)
        {
            if (character.Key.CompareTag("Player"))
                transform.position = character.Key.transform.position;
            character.Value.transform.position = character.Key.transform.position;
        }
    }
}
