using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllCharactersInfo : MonoBehaviour
{
    public Transform player, npc;
    public CharacterBehaviour playerBehaviour, npcBehaviour;
    public CharacterMoverController playerMover, npcMover;
    public Rigidbody playerRigidbody, npcRigidbody;
    public CollisionDetectionMode playerDetectionMode, npcDetectionMode;

    private void Awake()
    {
        player = transform.Find("Player");
        playerBehaviour = player.GetComponent<CharacterBehaviour>();
        playerMover = player.GetComponentInChildren<CharacterMoverController>();
        playerRigidbody = player.GetComponentInChildren<Rigidbody>();
        playerDetectionMode = playerRigidbody.collisionDetectionMode;

        npc = transform.Find("NPC");
        npcBehaviour = npc.GetComponent<CharacterBehaviour>();
        npcMover = npc.GetComponentInChildren<CharacterMoverController>();
        npcRigidbody = player.GetComponentInChildren<Rigidbody>();
        npcDetectionMode = npcRigidbody.collisionDetectionMode;

        switch (GameObject.Find("MultilevelController").GetComponent<MultilevelController>().playMode)
        {
            case MultilevelController.GamePlayMode.Normal:
            case MultilevelController.GamePlayMode.NN_REC:
                playerBehaviour.characterType = CharacterBehaviour.Type.Player;
                break;
            case MultilevelController.GamePlayMode.GA:
                playerBehaviour.characterType = CharacterBehaviour.Type.GA_NPC;
                break;
            case MultilevelController.GamePlayMode.NN:
                playerBehaviour.characterType = CharacterBehaviour.Type.NN_NPC;
                break;
            default:
                break;
        }
    }

    public void GetAll(out Transform p,
                       out CharacterBehaviour pb,
                       out CharacterMoverController pmc,
                       out Rigidbody prb,
                       out CollisionDetectionMode pcdm,
                       out Transform n,
                       out CharacterBehaviour nb,
                       out CharacterMoverController nmc,
                       out Rigidbody nrb,
                       out CollisionDetectionMode ncdm)
    {
        p = player;
        pb = playerBehaviour;
        pmc = playerMover;
        prb = playerRigidbody;
        pcdm = playerDetectionMode;

        n = npc;
        nb = npcBehaviour;
        nmc = npcMover;
        nrb = npcRigidbody;
        ncdm = npcDetectionMode;
    }
}