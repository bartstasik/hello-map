import tensorflow as tf
import numpy as np
import pandas as pd
import os

from tensorflow.python.keras import Sequential, Model
from tensorflow.python.keras.layers import Dense, TimeDistributed, LSTM, GRU, SimpleRNN, Input, concatenate, Dropout, \
    GaussianDropout
from tensorflow.python.keras.activations import relu
from tensorflow.python.keras.losses import SparseCategoricalCrossentropy
from tensorflow.python.saved_model import builder

DATASET_INPUTS = []
DATASET_LABELS = []

for i in range(5):
    data = pd.read_csv("test-person-%s.csv" % i)
    data = data[~data['rotation'].isin(['nan'])]
    data = data[~data['distanceFromPlayer'].isin(['nan'])]
    data = data.dropna(axis='columns')

    data.replace(False, 0, inplace=True)
    data.replace(True, 1, inplace=True)

    inputs = data.filter(items=['rotation',
                                'northRay',
                                'northwestRay',
                                'northeastRay',
                                'eastRay',
                                'southRay',
                                'westRay',
                                'doorClosed',
                                'distanceFromPlayer',
                                'seesKey']).values
    labels = data.filter(items=['mouseRotation',
                                'forwardButtonPressed',
                                'backButtonPressed',
                                'leftButtonPressed',
                                'rightButtonPressed',
                                'keyButtonPressed']).values

    DATASET_INPUTS.append(inputs)
    DATASET_LABELS.append(labels)

# inputs = DATASET.filter(items=['timestamp',
#                                'rotation',
#                                'northRay',
#                                'northwestRay',
#                                'northeastRay',
#                                'eastRay',
#                                'southRay',
#                                'westRay',
#                                'fitness',
#                                'doorClosed',
#                                'checkpointMet',
#                                'distanceFromPlayer',
#                                'seesKey'])


# inputs = DATASET.filter(items=['northRay',
#                                'northwestRay',
#                                'northeastRay',
#                                'eastRay',
#                                'southRay',
#                                'westRay']).values
# labels = DATASET.filter(items=['forwardButtonPressed',
#                                'backButtonPressed',
#                                'leftButtonPressed',
#                                'rightButtonPressed']).values
#

inputs = []
labels = []

for i in range(5):
    input_pad = np.zeros_like(DATASET_INPUTS[4])
    label_pad = np.zeros_like(DATASET_LABELS[4])
    input_pad[:DATASET_INPUTS[i].shape[0], :DATASET_INPUTS[i].shape[1]] = DATASET_INPUTS[i]
    label_pad[:DATASET_LABELS[i].shape[0], :DATASET_LABELS[i].shape[1]] = DATASET_LABELS[i]

    inputs.append(input_pad)
    labels.append(label_pad)

inputs = np.array(inputs)
labels = np.array(labels)

input_seq, input_timesteps, input_feat = inputs.shape
label_seq, label_timesteps, label_feat = labels.shape


def lstm(inputs, labels):
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    model = Sequential()

    # model.add(Dense(16, activation=relu, input_shape=(10,)))
    model.add(LSTM(64, stateful=True, return_sequences=True, batch_input_shape=(
        1, None, input_feat)))  # todo: variable timesteps, and save previous timesteps as progressing in-game
    # model.add(SimpleRNN(128))
    model.add(Dense(128, activation='relu'))
    model.add(Dense(label_feat, activation='relu'))

    model.compile(optimizer='sgd',
                  loss='mean_squared_error',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=250)

    model.save('deep with skip 73 percent.h5')

    print()


def dense(inputs, labels):
    model = Sequential()

    model.add(Dense(64, activation='relu', input_shape=(input_feat,)))
    model.add(Dense(128, activation='relu'))
    model.add(Dense(label_feat, activation='sigmoid'))

    model.compile(optimizer='adam',
                  loss='mean_squared_error',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=10)

    array = np.array([[0, 0, 0, 0, 0, 0]])
    # array = array.reshape((1, 1, array.size))
    a = model.predict(array)
    print()


def functional(inputs, labels):
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    input_0 = Input(batch_shape=(None, None, input_feat))
    input_1 = LSTM(128, stateful=True, return_sequences=True)(input_0)
    input_2 = Dense(64, activation='relu')(input_1)
    input_2 = Dropout(rate=0.1)(input_2)
    input_3 = Dense(32, activation='relu')(concatenate([input_1, input_2]))
    input_3 = Dropout(rate=0.1)(input_3)
    prediction = Dense(label_feat, activation='sigmoid')(concatenate([input_1, input_2, input_3]))

    model = Model(inputs=input_0, outputs=prediction)
    model.compile(optimizer='sgd',
                  loss='mean_squared_error',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=250)

    model.save('model.h5')

    print()


def functional2(inputs, labels):
    # inputs = inputs.reshape((input_seq, input_timesteps, input_feat))
    # labels = labels.reshape((input_seq, input_timesteps, input_feat))

    input_0 = Input(batch_shape=(input_seq, None, input_feat))
    input_1 = LSTM(128, stateful=True, return_sequences=True)(input_0)
    input_2 = Dense(64, activation='relu')(input_1)
    prediction = Dense(label_feat, activation='sigmoid')(concatenate([input_1, input_2]))

    model = Model(inputs=input_0, outputs=prediction)
    model.compile(optimizer='sgd',
                  loss='mean_squared_error',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=20, batch_size=5)

    model.save('model.h5')

    print()


def functional(inputs, labels):
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    input_0 = Input(batch_shape=(input_seq, None, input_feat))
    input_1 = LSTM(128, stateful=True, return_sequences=True)(input_0)
    input_2 = Dense(64, activation='relu')(input_1)
    input_2 = GaussianDropout(rate=0.1)(input_2)
    input_3 = Dense(32, activation='relu')(concatenate([input_1, input_2]))
    input_3 = Dropout(rate=0.1)(input_3)
    prediction = Dense(label_feat, activation='sigmoid')(concatenate([input_1, input_2, input_3]))

    model = Model(inputs=input_0, outputs=prediction)
    model.compile(optimizer='sgd',
                  loss='mean_squared_error',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=20, batch_size=5)  # , shuffle=True)

    model.save('model.h5')

    print()


def non_stateful(inputs, labels):
    input_0 = Input(shape=(None, input_feat))
    input_1 = LSTM(3, stateful=False, return_sequences=True)(input_0)
    input_2 = Dense(12, activation='relu')(input_1)
    input_2 = GaussianDropout(rate=0.1)(input_2)
    input_3 = Dense(32, activation='relu')(concatenate([input_1, input_2]))
    input_3 = Dropout(rate=0.1)(input_3)
    prediction = Dense(label_feat, activation='sigmoid')(concatenate([input_1, input_2, input_3]))

    model = Model(inputs=input_0, outputs=prediction)
    model.compile(optimizer='sgd',
                  loss='mean_squared_error',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=250, batch_size=5)  # , shuffle=True)

    model.save('model.h5')

    print()

# lstm(inputs, labels)
non_stateful(inputs, labels)

# inputs = inputs.reshape((1, input_timesteps, input_feat))
model = tf.keras.models.load_model('model.h5')
array = np.array([[0, 0, 0, 0, 0, 0, 0, 0, 0, 0]], dtype=float)
array = array.reshape((1, 1, input_feat))
a = model.predict(array, batch_size=1)
b = model.predict(inputs)
print()
