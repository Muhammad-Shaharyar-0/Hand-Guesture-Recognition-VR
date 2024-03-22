using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text;
using System.IO;




public class GuestureDetector : MonoBehaviour
{
    public OVRSkeleton skeleton;
    public List<CustomGesture> gestures;
    public bool debugMode = true;

    public List<OVRBone> fingerBones;

    private CustomGesture previousGesture;

    public float threshold=0.1f;

    public Text guestername;

    bool hasInitalized= false;

    [SerializeField] GRServer server;

    private string filePath;
    // Start is called before the first frame update
    void Start()
    {
        filePath = "./finger_positions.csv";
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

    public void StartHandTracking( )
    {

        fingerBones = new List<OVRBone>(skeleton.Bones);
        previousGesture = new CustomGesture();
        hasInitalized = true;
        Debug.LogError("Initilized");

    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.S))
        {
            StartHandTracking();
        }

        if (Input.GetKeyDown(KeyCode.R))
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

        if (hasInitalized)
        {
            TrackHands();
        }

    }

    void TrackHands()
    {
        if (debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }


        //CustomGesture currentGesture = Recognize();
        //bool hasRecognized = !currentGesture.Equals(new CustomGesture());
        ////Check if new Gesture
        //if (hasRecognized && !currentGesture.Equals(previousGesture))
        //{
        //    Debug.LogError("New Gesture Found: " + currentGesture.name);
        //    guestername.text = currentGesture.name;
        //    previousGesture = currentGesture;
        //    currentGesture.onRecognized?.Invoke();
        //}
    }


    void Save()
    {
        CustomGesture g = new CustomGesture();
        g.name = "New Gesture";
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
        gestures.Add(g);
    }

    CustomGesture Recognize()
    {
        CustomGesture currentgesture = new CustomGesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i = 0; i < fingerBones.Count; i++)
            {
                Vector3 currendData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currendData, gesture.fingerPositions[i]);
                if (distance > threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentgesture = gesture;
            }
        }
        return currentgesture;
    }
}
