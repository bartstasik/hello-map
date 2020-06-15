import asyncio

import nn_message_pb2 as pb  # Auto-generated Protobuf class
import numpy as np
import tensorflow_core as tf
import tensorflow_core.python.keras.backend as kb
import websockets
from tensorflow_core.python.keras import Model
from tensorflow_core.python.keras.layers import Dense, LSTM, GaussianNoise, Activation, Input, \
    GaussianDropout


def convert_function(n):
    return n / 180 - 2 if n > 180 else n / 180


def custom_loss(x0, x):
    a = 1 - kb.tanh(x)
    b = 1 + kb.tanh(x)
    c_a = 1 - x0
    c_b = 1 + x0
    term_a = c_a * kb.log(a)
    term_b = c_b * kb.log(b)
    result = -0.5 * (term_a + term_b)
    return result


def lstm_functional():
    input = Input(batch_input_shape=(1, None, 7))
    layer_1 = LSTM(6, stateful=True, return_sequences=True)(input)
    layer_1 = GaussianNoise(0.5)(layer_1)
    layer_1 = Activation('tanh')(layer_1)
    layer_1 = GaussianDropout(0.2)(layer_1)
    output_mouse = Dense(1, activation='tanh', name='mouse', )(layer_1)
    output_w_key = Dense(1, activation='relu', name='w_key')(layer_1)

    model = Model(inputs=input, outputs=[output_mouse, output_w_key])

    losses = {
        "mouse": custom_loss,
        "w_key": "binary_crossentropy"
    }

    model.load_weights('model.h5')

    model.compile(optimizer='adam',
                  loss=losses,
                  metrics=['accuracy'])

    model.summary()

    return model


async def echo(websocket, path):
    async for message in websocket:
        a = pb.MovementInput()
        a.ParseFromString(message)

        inputs = np.array([[convert_function(a.rotation),
                            a.northRay,
                            a.northwestRay,
                            a.northeastRay,
                            a.eastRay,
                            a.westRay,
                            a.distanceFromPlayer]], dtype=float)

        inputs = inputs.reshape((1, 1, inputs.size))

        output = np.array(pre_model.predict(inputs))
        output = output.reshape(output.size)

        movement_output = pb.MovementOutput()
        movement_output.mouseRotation = output[0]
        movement_output.forwardButtonPressed = round(output[1])
        movement_output.backButtonPressed = False  # round(output[2])
        movement_output.leftButtonPressed = False  # round(output[3])
        movement_output.rightButtonPressed = False  # round(output[4])
        movement_output.keyButtonPressed = False  # round(output[5])

        await websocket.send(movement_output.SerializeToString())


tf.keras.backend.set_learning_phase(0)

pre_model = lstm_functional()

asyncio.get_event_loop().run_until_complete(
    websockets.serve(echo, 'localhost', 6969))

asyncio.get_event_loop().run_forever()
