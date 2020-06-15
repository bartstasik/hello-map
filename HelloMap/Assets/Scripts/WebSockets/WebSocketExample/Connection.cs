using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Google.Protobuf;
using UnityEngine;
using NativeWebSocket;
using Neuralnet;
using Google.Protobuf;

public class Connection : MonoBehaviour
{
    private WebSocket _websocket;
    private CharacterBehaviour _character;

    async void Start()
    {
        _character = GetComponent<CharacterBehaviour>();
        if (_character.characterType != CharacterBehaviour.Type.NN_NPC)
            return;


        _websocket = new WebSocket("ws://localhost:6969");

        _websocket.OnOpen += () => { Debug.Log("Connection open!"); };

        _websocket.OnError += (e) => { Debug.Log("Error! " + e); };

        _websocket.OnClose += (e) => { Debug.Log("Connection closed!"); };

        _websocket.OnMessage += (bytes) =>
        {
            Debug.Log("OnMessage! " + Deserialize(bytes));
        };

        await _websocket.Connect();
    }

    async void FixedUpdate()
    {
        if (_character.characterType != CharacterBehaviour.Type.NN_NPC || _websocket.State != WebSocketState.Open)
            return;
        
        await _websocket.Send(Serialize());
    }

    private static MovementInput Serialize()
    {
        var movementInput = new MovementInput();
        movementInput.Rotation = DataContainer.rotation.GetValueOrDefault(-1);
        movementInput.NorthRay = DataContainer.northRay.GetValueOrDefault(-1);
        movementInput.NorthwestRay = DataContainer.northwestRay.GetValueOrDefault(-1);
        movementInput.NortheastRay = DataContainer.northeastRay.GetValueOrDefault(-1);
        movementInput.EastRay = DataContainer.eastRay.GetValueOrDefault(-1);
        movementInput.SouthRay = DataContainer.southRay.GetValueOrDefault(-1);
        movementInput.WestRay = DataContainer.westRay.GetValueOrDefault(-1);
        movementInput.DoorClosed = DataContainer.doorClosed.GetValueOrDefault(false);
        movementInput.CheckpointMet = DataContainer.checkpointMet.GetValueOrDefault(-1);
        movementInput.DistanceFromPlayer = DataContainer.distanceFromPlayer.GetValueOrDefault(-1);
        movementInput.SeesKey = DataContainer.seesKey.GetValueOrDefault(false);
        return movementInput;
    }

    private string Deserialize(byte[] bytes)
    {
        var a = MovementOutput.Parser.ParseFrom(bytes);
        var vertical = 0;
        var horizontal = 0;
        
        if (a.ForwardButtonPressed)
            vertical = 1;
        else if (a.BackButtonPressed)
            vertical = -1;

        if (a.RightButtonPressed)
            horizontal = 1;
        else if (a.LeftButtonPressed)
            horizontal = -1;

        _character.MoveCharacter(
            pressKey: a.KeyButtonPressed,
            rotate: (float) a.MouseRotation,
            moveVertical: vertical,
            moveHorizontal: horizontal
        );
        
        return a.ToString();
    }
    
//    private static string Deserialize(byte[] bytes)
//    {
//        var a = MovementInput.Parser.ParseFrom(bytes);
//        return a.ToString();
//    }

    private async void OnApplicationQuit()
    {
        if (_character.characterType != CharacterBehaviour.Type.NN_NPC)
            return;
        await _websocket.Close();
    }
}