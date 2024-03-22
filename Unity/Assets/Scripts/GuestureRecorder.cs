using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text;
using System.IO;
using TMPro;

public class GuestureRecorder : MonoBehaviour
{
    public OVRSkeleton skeleton;
    public List<OVRBone> fingerBones;



    public List<CustomGesture> gestures;
    public List<CustomGesture> gesturesSamples;




    public TMP_InputField guesterSampleName;
    int sample = 1;
    public TMP_Text sampleNumber;
    string SampleName = "";
    public int numberofSamplesRequired = 3;


    bool hasInitalized = false;

    [SerializeField] GRServer server;

    private string filePath;
    public string pythonDirector;
    // Start is called before the first frame update
    void Start()
    {
        filePath = pythonDirector + "/finger_positions.csv"; ;
        WriteGestureDataToFile();
    }

    void WriteGestureDataToFile()
    {
        StringBuilder sb = new StringBuilder();

        // Header
        sb.Append("GestureName");
        for (int i = 0; i < gestures[0].fingerPositions.Count; i++)
        {
            sb.Append($",PosX_{i},PosY_{i},PosZ_{i},RotX_{i},RotY_{i},RotZ_{i}");
        }
        sb.AppendLine();

        // Data
        foreach (CustomGesture gesture in gestures)
        {
            sb.Append(gesture.name);
            for (int i = 0; i < gesture.fingerPositions.Count; i++)
            {
                Vector3 position = gesture.fingerPositions[i];
                Vector3 rotation = i < gesture.fingerRotations.Count ? gesture.fingerRotations[i] : Vector3.zero;

                sb.Append($",{position.x},{position.y},{position.z},{rotation.x},{rotation.y},{rotation.z}");
            }
            sb.AppendLine();
        }

        // Write to file
        File.WriteAllText(filePath, sb.ToString());
    }

    public void StartHandTracking()
    {

        fingerBones = new List<OVRBone>(skeleton.Bones);
        hasInitalized = true;
     //   Debug.LogError("Initilized");

    }

    // Update is called once per frame
    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    StartHandTracking();
        //}
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddGuestureSample();
        }
        if (AppManager.Instance.GuestureDetectorScreen.activeSelf == true)
        {
            CustomGesture g = new CustomGesture();
            g.name = "Unknown Guesture";
            List<Vector3> position = new List<Vector3>();
            List<Vector3> rotaton = new List<Vector3>();
            foreach (var bone in fingerBones)
            {
                //Can be done through multiple ways. This is finger position relative to root
                position.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
                rotaton.Add(skeleton.transform.InverseTransformDirection(bone.Transform.rotation.eulerAngles));

            }
            g.fingerPositions = position;
            g.fingerRotations = rotaton;

            server.PredictGesture(g);
        }


    }

    public void SetGuestureSampleName()
    {
        SampleName = guesterSampleName.text;
        sampleNumber.text = "Sample" + sample.ToString();
    }
    public void AddGuestureSample()
    {
        if (SampleName == "") return;
        CustomGesture g = new CustomGesture();
        g.name = SampleName;
        List<Vector3> position = new List<Vector3>();
        List<Vector3> rotaton = new List<Vector3>();
        foreach (var bone in fingerBones)
        {
            //Can be done through multiple ways. This is finger position relative to root
            position.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
            rotaton.Add(skeleton.transform.InverseTransformDirection(bone.Transform.rotation.eulerAngles));

        }
        g.fingerPositions = position;
        g.fingerRotations = rotaton;
        gesturesSamples.Add(g);
        sample++;
        sampleNumber.text = "Sample" + sample.ToString();
        if (sample == numberofSamplesRequired+1)
        {
            AppManager.Instance.SamplesRecorded();
        }
       
    }

    public void RemoveSamples()
    {
        gesturesSamples.Clear();
        guesterSampleName.text = "Not Set";
        sample = 1;
        sampleNumber.text = "Sample" + sample.ToString();
    }
    public void AddGuesture()
    {
        foreach (var item in gesturesSamples)
        {
            gestures.Add(item);
        }
        gesturesSamples.Clear();
        guesterSampleName.text = "Not Set";
        sample=1;
        sampleNumber.text = "Sample" + sample.ToString();
        WriteGestureDataToFile();
    }
}
