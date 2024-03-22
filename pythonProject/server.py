from flask import Flask, request, jsonify
import numpy as np
import tensorflow as tf
from sklearn.preprocessing import StandardScaler
import joblib
import main

# Load the scaler
scaler = joblib.load('scaler.pkl')

app = Flask(__name__)

# Load the trained CNN model
cnn_model = tf.keras.models.load_model('gesture_recognition_model_positions_only.h5')

# Load the scaler used for normalizing features
scaler = joblib.load('scaler.pkl')
label_encoder = joblib.load('label_encoder.pkl')

@app.route('/predict', methods=['POST'])
def predict():
    data = request.json
    if 'command' in data:
        # Handle command messages
        command = data['command']
        if command == "Retrain model":
            # Retraining logic here
            print("Retraining the model...")
            main.train_gesture_recognition_model()
        return jsonify({'message': 'Data processed'})
        # Add other command checks if necessary
    else:
        features = np.array(data['features'])

        # Example single sample with 72 features
        single_sample = np.array([features]).reshape(1, -1)  # feature_values should be an array of 72 elements

        # Normalize the features using the loaded scaler
        single_sample_normalized = scaler.transform(single_sample)

        # Make a prediction
        predictions = cnn_model.predict(single_sample_normalized)
        confidence_threshold = 0.7  # For example, 70%

        # Analyze predictions
        for i, prediction in enumerate(predictions):
            predicted_class = np.argmax(prediction)
            confidence_level = prediction[predicted_class]

            if confidence_level < confidence_threshold:
                print(f"Sample {i}: No gesture (Confidence {confidence_level:.2f})")
                response = {'message': 'No guesture'}
            else:
                print(f"Sample {i}: Gesture {predicted_class} (Confidence {confidence_level:.2f})")
                # Process the prediction (e.g., convert to class label)
                predicted_class = np.argmax(predictions, axis=1)[0]
                predicted_gesture_name = label_encoder.inverse_transform([predicted_class])[0]
                response = {'message': predicted_gesture_name}


        return jsonify(response)

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)
