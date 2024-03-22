using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[System.Serializable]
public class CommandData
{
    public string command;
}

[System.Serializable]
public class ServerResponse
{
    public string message;
}

public class GRServer : MonoBehaviour
{
    private string apiUrl = "http://192.168.1.195:5000/predict";

    // Updated to accept CustomGesture object
    public void PredictGesture(CustomGesture gesture)
    {
        string jsonGesture = gesture.ToJson(); // Convert the gesture to JSON format
        StartCoroutine(SendRequest(jsonGesture));
    }
    public void TrainGuesture()
    {
        CommandData data = new CommandData { command = "Retrain model" };
        string jsonData = JsonUtility.ToJson(data);

        StartCoroutine(SendRequest(jsonData));
    }

    IEnumerator SendRequest(string jsonGesture)
    {
        // Create a new UnityWebRequest with the JSON data
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonGesture);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                // Handle the response
                string responseText = request.downloadHandler.text;
                Debug.LogError("Server response: " + responseText);             
                ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(responseText);
                if (serverResponse.message == "Data processed")
                {
                    AppManager.Instance.ModelRetrained();
                }
                else
                {
                    
                    AppManager.Instance.CurrentGuestureDectected(serverResponse.message);
                }          
                // Further processing...
            }
        }
    }
}
