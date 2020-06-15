import os

import numpy as np
import pandas as pd
import tensorflow_core.python.keras.backend as kb
from matplotlib import pyplot
from numpy import load, save, asarray
from tensorflow_core.python.keras import Sequential, Model
from tensorflow_core.python.keras.callbacks import EarlyStopping
from tensorflow_core.python.keras.layers import Dense, SimpleRNN, Input, concatenate, \
    GaussianDropout, CuDNNLSTM, GaussianNoise, Activation


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


def create_data(directory="data_fixed_ray/"):
    DATASET_INPUTS = []
    DATASET_LABELS = []
    count = 0
    for root, dirs, files in os.walk(directory):
        print(root)
        for file in files:
            if file.endswith(".csv") and file.startswith("output_"):
                try:
                    data = pd.read_csv(os.path.join(root, file))
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
                    count += 1
                except KeyError:
                    print(os.path.join(root, file))
    largest_data = 0
    largest_data_index = 0

    for i in range(len(DATASET_LABELS)):
        if len(DATASET_LABELS[i]) > largest_data:
            largest_data = len(DATASET_LABELS[i])
            largest_data_index = i

    inputs = []
    labels = []

    for i in range(len(DATASET_LABELS)):
        input_pad = np.zeros_like(DATASET_INPUTS[largest_data_index])
        label_pad = np.zeros_like(DATASET_LABELS[largest_data_index])
        input_pad[:DATASET_INPUTS[i].shape[0], :DATASET_INPUTS[i].shape[1]] = DATASET_INPUTS[i]
        label_pad[:DATASET_LABELS[i].shape[0], :DATASET_LABELS[i].shape[1]] = DATASET_LABELS[i]

        inputs.append(input_pad)
        labels.append(label_pad)

    inputs = np.array(inputs)
    labels = np.array(labels)

    np.random.seed(69)
    np.random.shuffle(inputs)
    np.random.shuffle(labels)

    save(os.path.join(directory, 'data-inputs-count-%s.npy' % count), asarray(inputs))
    save(os.path.join(directory, 'data-labels-count-%s.npy' % count), asarray(labels))

    return inputs, labels


def load_data(directory="data_fixed_ray/", count="4126"):
    return load(os.path.join(directory, 'data-inputs-count-%s.npy' % count)), \
           load(os.path.join(directory, 'data-labels-count-%s.npy' % count))


def lstm_functional(inputs, labels, patience):
    input = Input(batch_input_shape=(256, None, input_feat))
    layer_1 = CuDNNLSTM(6, stateful=True, return_sequences=True)(input)
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

    model.compile(optimizer='adam',
                  loss=losses,
                  metrics=['accuracy'])

    model.summary()

    labels_ = labels[:, :, 0:1]
    labels_1 = labels[:, :, 1:2]
    history = model.fit(inputs[:], [labels_, labels_1], epochs=1000, shuffle=True,
                        validation_split=0.25, verbose=1,
                        callbacks=[EarlyStopping(monitor='val_loss',
                                                 min_delta=0,
                                                 patience=patience,
                                                 verbose=0,
                                                 mode='auto',
                                                 baseline=0.89,
                                                 restore_best_weights=True)])

    model.save('model.h5')

    pyplot.plot(history.history['mouse_loss'])
    pyplot.plot(history.history['val_mouse_loss'])
    pyplot.plot(history.history['w_key_loss'])
    pyplot.plot(history.history['val_w_key_loss'])
    pyplot.plot(history.history['loss'])
    pyplot.plot(history.history['val_loss'])
    pyplot.title('model train vs validation loss')
    pyplot.ylabel('loss')
    pyplot.xlabel('epoch')
    pyplot.legend(['loss', 'mouse', 'w_key', 'val_loss', 'mouse_val', 'w_key_val'], loc='upper right')
    pyplot.show()


def rnn_func(inputs, labels):
    input_0 = Input(batch_shape=(1, None, input_feat))
    input_1 = SimpleRNN(128, stateful=True, return_sequences=True)(input_0)
    input_1 = GaussianNoise(0.5)(input_1)
    input_1 = Activation('tanh')(input_1)
    input_1 = GaussianDropout(0.2)(input_1)
    prediction_0 = Dense(1, activation='tanh', name='mouse')(input_1)
    prediction_1 = Dense(label_feat - 1, activation='relu', name='w_key')(input_1)

    output = concatenate([prediction_0, prediction_1])

    model = Model(inputs=input_0, outputs=output)
    model.compile(optimizer='sgd',
                  loss='binary_crossentropy',
                  metrics=['accuracy'])

    model.summary()

    model.fit(inputs, labels, epochs=20)

    model.save('model.h5')


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

    model.save('model.h5')


inputs, labels = create_data()
# inputs, labels = load_data()

inputs = inputs[:4096, :, :]
labels = labels[:4096, :, :]

input_seq, input_timesteps, input_feat = inputs.shape
label_seq, label_timesteps, label_feat = labels.shape

lstm_functional(inputs, labels, 10)
