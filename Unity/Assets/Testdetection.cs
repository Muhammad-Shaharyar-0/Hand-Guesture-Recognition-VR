using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;

public class Testdetection : MonoBehaviour
{
    [SerializeField] string fileName = "model.tflite";


    Interpreter interpreter;

    void Start()
    {
        var options = new InterpreterOptions()
        {
            threads = 2,
        };

       // string path = Path.Combine(Application.streamingAssetsPath, fileName);
        interpreter = new Interpreter(FileUtil.LoadFile(fileName), options);
        interpreter.AllocateTensors();
     
    }

    void OnDestroy()
    {
        interpreter?.Dispose();
    }

    public void OnTextChanged(string text)
    {
        //interpreter.SetInputTensorData(0, input);

    }
}