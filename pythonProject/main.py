import numpy as np
import pandas as pd
from sklearn.preprocessing import LabelEncoder, StandardScaler
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report, confusion_matrix
import tensorflow as tf
import joblib
import matplotlib.pyplot as plt
from sklearn.metrics import f1_score, accuracy_score
from sklearn.metrics import precision_recall_curve

def train_gesture_recognition_model():
    ## Read the data
    file_path = 'finger_positions.csv'  # Update with the actual path to your CSV file
    df = pd.read_csv(file_path)

    # Drop any rows that might have NaN values to avoid errors
    df = df.dropna()

    # Assuming there are 24 bones, and each bone has only position data (PosX, PosY, PosZ)
    num_bones = 24
    num_position_features = 3  # Only PosX, PosY, PosZ

    # Prepare column names for position data
    position_columns = []
    for i in range(num_bones):
        position_columns.extend([f'PosX_{i}', f'PosY_{i}', f'PosZ_{i}'])

    # Normalize position features
    scaler = StandardScaler()
    df[position_columns] = scaler.fit_transform(df[position_columns])

    # Save the scaler to a file
    joblib.dump(scaler, 'scaler.pkl')

    # Encode gesture names
    label_encoder = LabelEncoder()
    df['GestureLabel'] = label_encoder.fit_transform(df['GestureName'])

    joblib.dump(label_encoder, 'label_encoder.pkl')

    # Initialize the feature array
    X = df[position_columns].values

    # Labels
    y = df['GestureLabel'].values

    # One-hot encode labels
    y = tf.keras.utils.to_categorical(y, num_classes=len(label_encoder.classes_))

    # Split the data
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.5, stratify=y)


    # Neural network model
    model1 = tf.keras.Sequential([
       tf.keras.layers.Dense(128, activation='relu', input_shape=(X_train.shape[1],)),
        tf.keras.layers.Dense(64, activation='relu'),
       tf.keras.layers.Dropout(0.5),
       tf.keras.layers.Dense(y_train.shape[1], activation='softmax')
   ])

    # Compile Model 1
    model1.compile(optimizer='adam', loss='categorical_crossentropy', metrics=['accuracy'])
    # Train Model 1
    history1 = model1.fit(X_train, y_train, epochs=50, validation_data=(X_test, y_test))

    model2 = tf.keras.Sequential([
        tf.keras.layers.Dense(64, activation='relu', input_shape=(X_train.shape[1],)),
        tf.keras.layers.Dropout(0.3),
        tf.keras.layers.Dense(32, activation='relu'),
        tf.keras.layers.Dropout(0.3),
        tf.keras.layers.Dense(y_train.shape[1], activation='softmax')
    ])

    # Compile the model
    model2.compile(optimizer='adam', loss='categorical_crossentropy', metrics=['accuracy'])

    # Train the model
    history2 = model2.fit(X_train, y_train, epochs=50, validation_data=(X_test, y_test))

    # Save the model
    model1.save('gesture_recognition_model_positions_only.h5')

    # Evaluate models
    y_pred1 = model1.predict(X_test)
    y_pred1_classes = np.argmax(y_pred1, axis=1)
    y_pred2 = model2.predict(X_test)
    y_pred2_classes = np.argmax(y_pred2, axis=1)

    y_test_classes = np.argmax(y_test, axis=1)  # Assuming y_test is one-hot encoded

    # Calculate accuracy and F1-score
    accuracy1 = accuracy_score(y_test_classes, y_pred1_classes)
    f1_score1 = f1_score(y_test_classes, y_pred1_classes, average='weighted')

    accuracy2 = accuracy_score(y_test_classes, y_pred2_classes)
    f1_score2 = f1_score(y_test_classes, y_pred2_classes, average='weighted')

    print(f"Model 1 - Accuracy: {accuracy1}, F1-Score: {f1_score1}")
    print(f"Model 2 - Accuracy: {accuracy2}, F1-Score: {f1_score2}")


    plt.plot(history1.history['loss'], label='Model 1 Training Loss')
    plt.plot(history1.history['val_loss'], label='Model 1 Validation Loss')
    plt.plot(history2.history['loss'], label='Model 2 Training Loss')
    plt.plot(history2.history['val_loss'], label='Model 2 Validation Loss')
    plt.title('Model Loss')
    plt.ylabel('Loss')
    plt.xlabel('Epoch')
    plt.legend()
    plt.show()

    # Evaluate the models
    loss1, accuracy1 = model1.evaluate(X_test, y_test, verbose=0)
    loss2, accuracy2 = model2.evaluate(X_test, y_test, verbose=0)

    # Print results
    print("Model 1 - Accuracy: {:.2f}, Loss: {:.2f}".format(accuracy1, loss1))
    print("Model 2 - Accuracy: {:.2f}, Loss: {:.2f}".format(accuracy2, loss2))

    # Print overfitting analysis
    for metric in ['accuracy', 'loss']:
        print("\n{} - Model 1 vs Model 2".format(metric.capitalize()))
        for epoch in range(len(history1.history[metric])):
            print(
                f"Epoch {epoch + 1}: Model 1 - {history1.history[metric][epoch]:.2f}, Model 2 - {history2.history[metric][epoch]:.2f}")

    # Plotting Accuracy and Loss
    fig, axes = plt.subplots(1, 2, figsize=(15, 5))

    # Accuracy
    axes[0].plot(history1.history['accuracy'], label='Model 1 Training Accuracy')
    axes[0].plot(history1.history['val_accuracy'], label='Model 1 Validation Accuracy')
    axes[0].plot(history2.history['accuracy'], label='Model 2 Training Accuracy')
    axes[0].plot(history2.history['val_accuracy'], label='Model 2 Validation Accuracy')
    axes[0].set_title('Model Accuracy')
    axes[0].set_ylabel('Accuracy')
    axes[0].set_xlabel('Epoch')
    axes[0].legend()

    # Loss
    axes[1].plot(history1.history['loss'], label='Model 1 Training Loss')
    axes[1].plot(history1.history['val_loss'], label='Model 1 Validation Loss')
    axes[1].plot(history2.history['loss'], label='Model 2 Training Loss')
    axes[1].plot(history2.history['val_loss'], label='Model 2 Validation Loss')
    axes[1].set_title('Model Loss')
    axes[1].set_ylabel('Loss')
    axes[1].set_xlabel('Epoch')
    axes[1].legend()

    plt.show()


train_gesture_recognition_model()