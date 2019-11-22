using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalCharacter : MonoBehaviour
{
    public GameObject[] checkpoints;

    [NonSerialized] public short playerCheckpoint;
    [NonSerialized] public short npcCheckpoint;
}