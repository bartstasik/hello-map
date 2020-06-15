using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelSpawner : MonoBehaviour
{
    [SerializeField] private Vector3 playerPosition;
    [SerializeField] private Quaternion playerRotation;
    [SerializeField] private Vector3 npcPosition;
    [SerializeField] private Quaternion npcRotation;

    [SerializeField] private Vector2 minRange;
    [SerializeField] private Vector2 maxRange;

    [NonSerialized] public MinimapCameraController minimap;

    private AllCharactersInfo _allCharactersTemplate;
    private CheckpointController2 _checkpointController2;

    public void ConfigureLevel(AllCharactersInfo allCharacters)
    {
        _allCharactersTemplate = allCharacters;
        _checkpointController2 = GetComponentInChildren<CheckpointController2>();
        _checkpointController2.LocateCheckpoints();
        minimap = GameObject.FindGameObjectWithTag("Minimap")
                            ?.GetComponent<MinimapCameraController>();
        Cursor.visible = false;
//        characters[0] = NewPair(checkpointController.checkpoints);

//        for (int i = 1; i < population - 1; i++)
//            characters[i] = Pair(checkpointController.checkpoints,
//                                 playerPosition, playerRotation,
//                                 npcPosition, npcRotation);
//            characters[i] = NewPair(checkpointController.checkpoints);
    }

    public CharacterBehaviour MoveExistingPair(bool randomised = false)
    {
        if (randomised)
        {
            playerPosition = new Vector3(Random.Range(minRange.x, maxRange.x),
                                         playerPosition.y,
                                         Random.Range(minRange.y, maxRange.y));
            playerRotation = new Quaternion(playerRotation.x, 
                                            Random.rotation.y, 
                                            playerRotation.z, 
                                            playerRotation.w);
        }

        return Pair(_allCharactersTemplate,
                    _checkpointController2.checkpoints,
                    playerPosition, playerRotation,
                    npcPosition, npcRotation,
                    false);
    }

    public CharacterBehaviour NewPair()
    {
        return Pair(Instantiate(_allCharactersTemplate, Vector3.zero, Quaternion.identity),
                    _checkpointController2.checkpoints,
                    playerPosition, playerRotation,
                    npcPosition, npcRotation);
    }

    private CharacterBehaviour Pair(AllCharactersInfo allCharacters,
                                    Checkpoint[] checkpoints,
                                    Vector3 position1,
                                    Quaternion rotation1,
                                    Vector3 position2,
                                    Quaternion rotation2,
                                    bool newPair = true)
    {
//        Transform player, npc;
//        CharacterBehaviour playerBehaviour, npcBehaviour;
//        CharacterMoverController playerMover, npcMover;
//        Rigidbody playerRigidbody, npcRigidbody;
//
//        allCharacters.GetAll(out player,
//                             out playerBehaviour,
//                             out playerMover,
//                             out playerRigidbody,
//                             out npc,
//                             out npcBehaviour,
//                             out npcMover,
//                             out npcRigidbody);

//        if (allCharacters.playerBehaviour.characterType.Equals(CharacterBehaviour.Type.GA_NPC))
//            allCharacters.playerBehaviour.allCheckpoints = checkpoints;

        allCharacters.playerBehaviour.allCheckpoints = checkpoints;
        allCharacters.npcBehaviour.allCheckpoints = checkpoints;

        allCharacters.gameObject.SetActive(true);

        allCharacters.playerRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        allCharacters.playerRigidbody.isKinematic = true;
        allCharacters.player.position = position1;
        allCharacters.player.rotation = rotation1;
        allCharacters.playerRigidbody.isKinematic = false;
        allCharacters.playerRigidbody.collisionDetectionMode = allCharacters.playerDetectionMode;

        allCharacters.npcRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        allCharacters.npcRigidbody.isKinematic = true;
        allCharacters.npc.position = position2;
        allCharacters.npc.rotation = rotation2;
        allCharacters.npcRigidbody.isKinematic = false;
        allCharacters.npcRigidbody.collisionDetectionMode = allCharacters.npcDetectionMode;

        minimap?.AddCharacter(allCharacters.playerMover,
                              allCharacters.playerBehaviour);
        minimap?.AddCharacter(allCharacters.npcMover,
                              allCharacters.npcBehaviour);

        return allCharacters.playerBehaviour;
    }
}