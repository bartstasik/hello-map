import tensorflow as tf
import numpy as np
import pandas as pd
import os

from tensorflow.python.keras import Sequential, Model
from tensorflow.python.keras.layers import Dense, TimeDistributed, LSTM, GRU, SimpleRNN, Input, concatenate, Dropout, \
    GaussianDropout, merge, CuDNNLSTM
from tensorflow.python.keras.activations import relu
from tensorflow.python.keras.losses import SparseCategoricalCrossentropy
from tensorflow.python.saved_model import builder
from tensorflow.python.keras.callbacks import EarlyStopping
from tensorflow.python.client import device_lib

print(tf.test.is_built_with_cuda())
print(device_lib.list_local_devices())

DATASET_INPUTS = []
DATASET_LABELS = []


def convert_function(n):
    return n / 180 - 2 if n > 180 else n / 180


# for i in range(117):
# for i in range(601, 1002):
# for i in range(0, 1002):
# for i in range(1002,1101):
for i in range(1002,1201):
# for i in range(1201):
# for i in range(61):
    # data = pd.read_csv("data/simple-straight/test-simple-straight-%s.csv" % i)
    # data = pd.read_csv("data/simple-obstacle/test-simple-obstacle-%s.csv" % i)
    # data = pd.read_csv("data/simple-obstacle-lookat/output_%s.csv" % i)
    try:
        data = pd.read_csv("data_fixed_ray/simple-obstacle-lookat/output_%s.csv" % i)
        data = data[(~data['rotation'].isin(['nan']))]
        data['rotation'] = data['rotation'].apply(lambda x: convert_function(x))
        data = data[~data['northRay'].isin(['nan'])]
        data = data[~data['northwestRay'].isin(['nan'])]
        data = data[~data['northeastRay'].isin(['nan'])]
        data = data[~data['eastRay'].isin(['nan'])]
        data = data[~data['southRay'].isin(['nan'])]
        data = data[~data['westRay'].isin(['nan'])]
        data = data[~data['doorClosed'].isin(['nan'])]
        data = data[~data['checkpointMet'].isin(['nan'])]
        data = data[~data['distanceFromPlayer'].isin(['nan'])]
        data = data[~data['seesKey'].isin(['nan'])]
        data = data.dropna(axis='columns')

        data.replace(False, 0, inplace=True)
        data.replace(True, 1, inplace=True)

        inputs = data.filter(items=['rotation',
                                    'northRay',
                                    'northwestRay',
                                    'northeastRay',
                                    'eastRay',
                                    'westRay',
                                    'distanceFromPlayer']).values
        labels = data.filter(items=['mouseRotation',
                                    'forwardButtonPressed']).values

        DATASET_INPUTS.append(inputs)
        DATASET_LABELS.append(labels)
    except KeyError:
        print(i)

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

largest_data = 0
largest_data_index = 0

for i in range(len(DATASET_LABELS)):
    if len(DATASET_LABELS[i]) > largest_data:
        largest_data = len(DATASET_LABELS[i])
        largest_data_index = i

inputs = []
labels = []

for i in range(len(DATASET_LABELS)):
    # input_pad = np.zeros_like(DATASET_INPUTS[2])
    # label_pad = np.zeros_like(DATASET_LABELS[2])
    # input_pad = np.zeros_like(DATASET_INPUTS[21])
    # label_pad = np.zeros_like(DATASET_LABELS[21])
    input_pad = np.zeros_like(DATASET_INPUTS[largest_data_index])
    label_pad = np.zeros_like(DATASET_LABELS[largest_data_index])
    input_pad[:DATASET_INPUTS[i].shape[0], :DATASET_INPUTS[i].shape[1]] = DATASET_INPUTS[i]
    label_pad[:DATASET_LABELS[i].shape[0], :DATASET_LABELS[i].shape[1]] = DATASET_LABELS[i]

    inputs.append(input_pad)
    labels.append(label_pad)

inputs = np.array(inputs)
labels = np.array(labels)

input_seq, input_timesteps, input_feat = inputs.shape
label_seq, label_timesteps, label_feat = labels.shape


def lstm(inputs, labels, patience):
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    model = Sequential()

    # model.add(Dense(16, activation=relu, input_shape=(10,)))
    model.add(LSTM(64, stateful=True, return_sequences=True, batch_input_shape=(1, None, input_feat)))
    # todo: variable timesteps, and save previous timesteps as progressing in-game
    # model.add(SimpleRNN(128))
    model.add(Dense(128, activation='relu'))
    model.add(Dense(label_feat, activation='relu'))

    model.compile(optimizer='sgd',
                  loss='mean_squared_error',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=1000, shuffle=True,
              callbacks=[EarlyStopping(monitor='loss',
                                       min_delta=0,
                                       patience=patience,
                                       verbose=0,
                                       mode='auto',
                                       baseline=None,
                                       restore_best_weights=True)])

    model.save('model.h5')

    print()


def lstm_simple(inputs, labels, patience):
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    model = Sequential()

    # model.add(Dense(16, activation=relu, input_shape=(10,)))
    model.add(CuDNNLSTM(6, stateful=True, return_sequences=True, batch_input_shape=(1, None, input_feat)))
    # model.add(Dense(128, activation='relu'))
    # model.add(SimpleRNN(128))
    model.add(Dense(label_feat, activation='tanh'))

    model.compile(optimizer='sgd',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=1000, shuffle=True,
              callbacks=[EarlyStopping(monitor='loss',
                                       min_delta=0,
                                       patience=patience,
                                       verbose=0,
                                       mode='auto',
                                       baseline=None,
                                       restore_best_weights=True)])

    model.save('model.h5')

    print()


def lstm_load(inputs, labels, patience, name):
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    model = Sequential()

    # model.add(Dense(16, activation=relu, input_shape=(10,)))
    model.add(LSTM(6, stateful=True, return_sequences=True, batch_input_shape=(1, None, 7)))
    # model.add(Dense(128, activation='relu'))
    # model.add(SimpleRNN(128))
    model.add(Dense(2, activation='tanh'))

    model.load_weights(name)

    model.compile(optimizer='sgd',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=1000, shuffle=True,
              callbacks=[EarlyStopping(monitor='loss',
                                       min_delta=0,
                                       patience=patience,
                                       verbose=0,
                                       mode='auto',
                                       baseline=None,
                                       restore_best_weights=True)])

    model.save('model.h5')

    return model


def rnn(inputs, labels, patience):
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    model = Sequential()

    # model.add(Dense(16, activation=relu, input_shape=(10,)))
    # model.add(SimpleRNN(6, stateful=True, return_sequences=True, batch_input_shape=(1, None, input_feat)))
    model.add(SimpleRNN(6, stateful=True, return_sequences=True, batch_input_shape=(1, None, input_feat)))
    # model.add(SimpleRNN(128))
    model.add(Dense(label_feat, activation='tanh'))

    model.compile(optimizer='sgd',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=1000, shuffle=True,
              callbacks=[EarlyStopping(monitor='loss',
                                       min_delta=0,
                                       patience=patience,
                                       verbose=0,
                                       mode='auto',
                                       baseline=None,
                                       restore_best_weights=True)])

    model.save('model.h5')

    print()


def rnn_load(inputs, labels, patience, name):
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    model = tf.keras.models.load_model(name)

    model.summary()

    model.fit(inputs, labels, epochs=1000, shuffle=True,
              callbacks=[EarlyStopping(monitor='loss',
                                       min_delta=0,
                                       patience=patience,
                                       verbose=0,
                                       mode='auto',
                                       baseline=None,
                                       restore_best_weights=True)])

    model.save('model.h5')

    print()


def lstm_new(inputs, labels, patience):
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    model = Sequential()

    # model.add(Dense(16, activation=relu, input_shape=(10,)))
    # model.add(SimpleRNN(6, stateful=True, return_sequences=True, batch_input_shape=(1, None, input_feat)))
    model.add(LSTM(64, stateful=True, return_sequences=True, batch_input_shape=(1, None, input_feat)))
    # model.add(SimpleRNN(128))
    model.add(Dense(128, activation='relu'))
    model.add(Dense(label_feat, activation='tanh'))

    model.compile(optimizer='sgd',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=1000, shuffle=True,
              validation_split=0.15,
              callbacks=[EarlyStopping(monitor='val_loss',
                                       min_delta=0,
                                       patience=patience,
                                       verbose=0,
                                       mode='auto',
                                       baseline=None,
                                       restore_best_weights=True)])

    model.save('model.h5')

    print()


def rnn_func(inputs, labels):
    # inputs = inputs.reshape((1, input_timesteps, input_num))
    # labels = labels.reshape((1, label_timesteps, label_num))

    model = Sequential()

    # model.add(Dense(16, activation=relu, input_shape=(10,)))
    model.add(SimpleRNN(6, stateful=True, return_sequences=True, batch_input_shape=(1, None, input_feat)))
    # model.add(SimpleRNN(128))
    model.add(Dense(label_feat, activation='tanh'))

    input_0 = Input(batch_shape=(1, None, input_feat))
    input_1 = SimpleRNN(128, stateful=True, return_sequences=True)(input_0)
    prediction_0 = Dense(1, activation='tanh')(input_1)
    prediction_1 = Dense(label_feat - 1, activation='relu')(input_1)

    output = concatenate([prediction_0, prediction_1])

    model = Model(inputs=input_0, outputs=output)
    model.compile(optimizer='sgd',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=20)

    model.save('model.h5')

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


def dense_3d(inputs, labels, patience):
    model = Sequential()

    model.add(Dense(64, activation='relu', batch_input_shape=(1, None, input_feat)))
    model.add(Dense(128, activation='relu'))
    model.add(Dense(label_feat, activation='relu'))

    model.compile(optimizer='adam',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=20, shuffle=True)
    # callbacks=[EarlyStopping(monitor='loss',
    #                          min_delta=0,
    #                          patience=patience,
    #                          verbose=0,
    #                          mode='auto',
    #                          baseline=None,
    #                          restore_best_weights=False)])

    model.save('model.h5')

    print()


def dense_3d_tanh(inputs, labels, patience):
    model = Sequential()

    model.add(Dense(64, activation='relu', batch_input_shape=(1, None, input_feat)))
    model.add(Dense(128, activation='relu'))
    model.add(Dense(label_feat, activation='tanh'))

    model.compile(optimizer='adam',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=1000, shuffle=True,
              callbacks=[EarlyStopping(monitor='loss',
                                       min_delta=0,
                                       patience=patience,
                                       verbose=0,
                                       mode='min',
                                       baseline=None,
                                       restore_best_weights=False)])

    model.save('model.h5')

    print()


def dense_3d_functional(inputs, labels, patience):
    input_0 = Input(batch_shape=(1, None, input_feat))
    input_1 = Dense(64, activation='relu')(input_0)
    output_0 = Dense(1, activation='tanh')(input_1)
    output_1 = Dense(label_feat - 1, activation='relu')(input_1)

    prediction = concatenate([output_0, output_1])

    model = Model(inputs=input_0, outputs=prediction)

    model.compile(optimizer='adam',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=1000, shuffle=True,
              callbacks=[EarlyStopping(monitor='loss',
                                       min_delta=0,
                                       patience=patience,
                                       verbose=0,
                                       mode='min',
                                       baseline=None,
                                       restore_best_weights=False)])

    model.save('model.h5')

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

    input_0 = Input(batch_shape=(1, None, input_feat))
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

    model.fit(inputs, labels, epochs=20, shuffle=True)

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


# lstm(inputs, labels, 10)
# rnn(inputs, labels, 10)
lstm_simple(inputs, labels, 10)
# rnn_load(inputs, labels, 10, 'BEST MODELS/rnn tanh bigger(117) data 90 percent retrain - better.h5')
# rnn_load(inputs, labels, 10, 'BEST MODELS/rnn tanh 117-retrain-better plus 484 data 89 percent.h5')
# rnn_load(inputs, labels, 10, 'rnn tanh 100 - only simple map 97 percent.h5')
# rnn_load(inputs, labels, 10, 'rnn tanh bigger data 75 percent.h5')
# lstm_load(inputs, labels, 10, 'model.h5')

# inputs = inputs.reshape((1, input_timesteps, input_feat))
# model = tf.keras.models.load_model('model.h5')
# array = np.array([[0, 0, 0, 0, 0, 0, 0]], dtype=float)
# array = array.reshape((1, 1, input_feat))
# a = model.predict(array, batch_size=1)
# b = model.predict(inputs)

print()
