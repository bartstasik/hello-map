using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCoordinates : MonoBehaviour
{
    [SerializeField] private Vector3 playerPosition;
    [SerializeField] private Quaternion playerRotation;
    [SerializeField] private Vector3 npcPosition;
    [SerializeField] private Quaternion npcRotation;

    private GameObject _allCharactersTemplate;

    public void ConfigureLevel(GameObject allCharacters)
    {
        _allCharactersTemplate = allCharacters;
        var checkpointController = GetComponentInChildren<CheckpointController2>();
        checkpointController.LocateCheckpoints();
        MoveExistingPair(playerPosition,
                         playerRotation,
                         npcPosition,
                         npcRotation,
                         checkpointController.checkpoints
        );
    }

    private void MoveExistingPair(Vector3 position1,
                                  Quaternion rotation1,
                                  Vector3 position2,
                                  Quaternion rotation2,
                                  Checkpoint[] checkpoints)
    {
        var player = _allCharactersTemplate.transform.Find("Player");
        var npc = _allCharactersTemplate.transform.Find("NPC");
        var playerMover = player.GetComponentInChildren<CharacterMoverController>();
        var npcMover = npc.GetComponentInChildren<CharacterMoverController>();
        var playerRigidbody = player.GetComponentInChildren<Rigidbody>();
        var npcRigidbody = player.GetComponentInChildren<Rigidbody>();
        var playerDetectionMode = playerRigidbody.collisionDetectionMode;
        var npcDetectionMode = npcRigidbody.collisionDetectionMode;
        
        npc.GetComponent<CharacterBehaviour>().allCheckpoints = checkpoints;

        _allCharactersTemplate.SetActive(true);

        playerRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        playerRigidbody.isKinematic = true;
        player.position = position1;
        player.rotation = rotation1;
        playerRigidbody.isKinematic = false;
        playerRigidbody.collisionDetectionMode = playerDetectionMode;

        npcRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        npcRigidbody.isKinematic = true;
        npc.position = position2;
        npc.rotation = rotation2;
        npcRigidbody.isKinematic = false;
        npcRigidbody.collisionDetectionMode = npcDetectionMode;

        gameObject.GetComponentInChildren<MinimapCameraController>()
                  .AddCharacter(playerMover);
        gameObject.GetComponentInChildren<MinimapCameraController>()
                  .AddCharacter(npcMover);
    }
}