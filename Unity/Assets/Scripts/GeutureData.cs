using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct CustomGesture
{
    public string name;
    public List<Vector3> fingerPositions;
    public List<Vector3> fingerRotations;
    public UnityEvent onRecognized;

    // Method to convert CustomGesture to a JSON string
    public string ToJson()
    {
        List<float> features = new List<float>();
        foreach (var position in fingerPositions)
        {
            features.Add(position.x);
            features.Add(position.y);
            features.Add(position.z);
        }

        //foreach (var rotation in fingerRotations)
        //{
        //    features.Add(rotation.x);
        //    features.Add(rotation.y);
        //    features.Add(rotation.z);
        //}

        GestureData gestureData = new GestureData
        {
            gestureName = name,
            features = features.ToArray()
        };

        string json = JsonUtility.ToJson(gestureData);
     //   Debug.LogError(json);  // For debugging
        return json;
    }
}

[System.Serializable]
public class GestureData
{
    public string gestureName;
    public float[] features;
}
