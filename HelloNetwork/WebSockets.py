import asyncio
import numpy as np
import tensorflow as tf
import websockets
from tensorflow.python.client import device_lib
from tensorflow.python.keras import Sequential
from tensorflow.python.keras.callbacks import EarlyStopping
from tensorflow.python.keras.layers import Dense, LSTM

import nn_message_pb2 as pb

print(tf.test.is_built_with_cuda())
print(device_lib.list_local_devices())


def convert_function(n):
    return n / 180 - 2 if n > 180 else n / 180


def lstm_simple():
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    model = Sequential()

    # model.add(Dense(16, activation=relu, input_shape=(10,)))
    model.add(LSTM(6, stateful=True, return_sequences=True, batch_input_shape=(1, None, 7)))
    # model.add(Dense(128, activation='relu'))
    # model.add(SimpleRNN(128))
    model.add(Dense(2, activation='tanh'))

    model.load_weights('model.h5')

    model.compile(optimizer='sgd',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    return model


async def echo(websocket, path):
    async for message in websocket:
        a = pb.MovementInput()
        a.ParseFromString(message)

        inputs = np.array([[convert_function(a.rotation),  # a.rotation,
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

        await websocket.send(movement_output.SerializeToString())


tf.keras.backend.set_learning_phase(0)

# pre_model = tf.keras.models.load_model('rnn tanh bigger data 75 percent.h5')
# pre_model = tf.keras.models.load_model('model.h5')
pre_model = lstm_simple()

asyncio.get_event_loop().run_until_complete(
    websockets.serve(echo, 'localhost', 6969))

asyncio.get_event_loop().run_forever()
