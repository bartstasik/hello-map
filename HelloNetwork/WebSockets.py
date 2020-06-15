import asyncio

import nn_message_pb2 as pb  # Auto-generated Protobuf class
import numpy as np
import tensorflow_core as tf
import tensorflow_core.python.keras.backend as kb
import websockets
<<<<<<< Updated upstream
from tensorflow_core.python.keras import Model
from tensorflow_core.python.keras.layers import Dense, LSTM, GaussianNoise, Activation, Input, \
    GaussianDropout
=======
import os
import math

# os.environ["CUDA_VISIBLE_DEVICES"] = "-1"
from tensorflow_core.python.client import device_lib
from tensorflow_core.python.keras import Sequential, Model
from tensorflow_core.python.keras.callbacks import EarlyStopping
from tensorflow_core.python.keras.layers import Dense, LSTM, GaussianNoise, Activation, BatchNormalization, Input, \
    concatenate, GaussianDropout, CuDNNLSTM

import nn_message_pb2 as pb

print(tf.test.is_built_with_cuda())
print(device_lib.list_local_devices())
>>>>>>> Stashed changes


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
<<<<<<< Updated upstream
=======


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


def lstm_simple_noisy():
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    model = Sequential()

    # model.add(Dense(16, activation=relu, input_shape=(10,)))
    model.add(LSTM(6, stateful=True, return_sequences=True, batch_input_shape=(1, None, 7), unit_forget_bias=True))
    # model.add(Dense(128, activation='relu'))
    # model.add(SimpleRNN(128))
    model.add(GaussianNoise(0.5))
    model.add(Activation('tanh'))
    # model.add(BatchNormalization())
    model.add(Dense(2, activation='tanh'))

    model.load_weights('model.h5')

    model.compile(optimizer='sgd',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    return model
>>>>>>> Stashed changes


def lstm_functional():
    input = Input(batch_input_shape=(1, None, 7))
    layer_1 = CuDNNLSTM(7, stateful=True, return_sequences=True)(input)
    layer_1 = GaussianNoise(0.5)(layer_1)
    layer_1 = Activation('tanh')(layer_1)
<<<<<<< Updated upstream
    layer_1 = GaussianDropout(0.2)(layer_1)
    output_mouse = Dense(1, activation='tanh', name='mouse', )(layer_1)
    output_w_key = Dense(1, activation='relu', name='w_key')(layer_1)
=======
    # layer_1 = GaussianDropout(0.2)(layer_1)
    output_mouse = Dense(1, activation='tanh', name='mouse', )(layer_1)
    output_w_key = Dense(1, activation='tanh', name='w_key')(layer_1)
>>>>>>> Stashed changes

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


def dense_3d():
    model = Sequential()

    model.add(Dense(6, activation='relu', batch_input_shape=(1, None, 7)))
    model.add(Dense(128, activation='relu'))
    model.add(Dense(2, activation='tanh'))

    model.load_weights('model.h5')

    model.compile(optimizer='adam',
                  loss='binary_crossentropy',
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
<<<<<<< Updated upstream
=======
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
>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
=======
        # print(output)

>>>>>>> Stashed changes
        await websocket.send(movement_output.SerializeToString())


tf.keras.backend.set_learning_phase(0)

<<<<<<< Updated upstream
pre_model = lstm_functional()
=======
# pre_model = tf.keras.models.load_model('rnn tanh bigger data 75 percent.h5')
# pre_model = tf.keras.models.load_model('model.h5')
# pre_model = lstm_simple_noisy()
# pre_model = dense_3d()
pre_model = lstm_functional()
inputs = [[[0, 0, 0, 0, 0, 0, 0]]]
pre_model.predict(inputs)
# pre_model = lstm_simple()
>>>>>>> Stashed changes

asyncio.get_event_loop().run_until_complete(
    websockets.serve(echo, 'localhost', 6969))

asyncio.get_event_loop().run_forever()
