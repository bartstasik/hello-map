import tensorflow as tf
import numpy as np
import pandas as pd
import asyncio, websockets, pickle, os, math
from tensorflow.python.saved_model import builder
import nn_message_pb2 as pb


async def echo(websocket, path):
    async for message in websocket:
        a = pb.MovementInput()
        a.ParseFromString(message)

        inputs = np.array([[a.rotation,
                            a.northRay,
                            a.northwestRay,
                            a.northeastRay,
                            a.eastRay,
                            a.westRay,
                            a.distanceFromPlayer]], dtype=float)
        # inputs = np.array([[a.rotation,
        #                     a.northRay,
        #                     a.northwestRay,
        #                     a.northeastRay,
        #                     a.eastRay,
        #                     a.southRay,
        #                     a.westRay,
        #                     a.doorClosed,
        #                     a.distanceFromPlayer,
        #                     a.seesKey]], dtype=float)
        inputs = inputs.reshape((1, 1, inputs.size))

        output = pre_model.predict(inputs)
        output = output.reshape(output.size)

        movement_output = pb.MovementOutput()

        # if output[0] < 0.1:
        #     movement_output.mouseRotation = 0
        # else:
        movement_output.mouseRotation = output[0]

        # if output[1] <= 0:
        #     movement_output.forwardButtonPressed = False
        # else:
        #     movement_output.forwardButtonPressed = True

        movement_output.forwardButtonPressed = round(output[1])
        movement_output.backButtonPressed = False  # round(output[2])
        movement_output.leftButtonPressed = False  # round(output[3])
        movement_output.rightButtonPressed = False  # round(output[4])
        movement_output.keyButtonPressed = False  # round(output[5])

        print(output)

        await websocket.send(movement_output.SerializeToString())


tf.keras.backend.set_learning_phase(0)

pre_model = tf.keras.models.load_model('model.h5')

asyncio.get_event_loop().run_until_complete(
    websockets.serve(echo, 'localhost', 6969))

asyncio.get_event_loop().run_forever()
