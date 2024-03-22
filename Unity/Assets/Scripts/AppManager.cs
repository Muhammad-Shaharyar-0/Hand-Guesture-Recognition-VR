using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AppManager : MonoBehaviour
{

    [SerializeField] GameObject HandTrackingScreen;
    [SerializeField] GameObject GuestureRecordingScreen;
    public GameObject GuestureDetectorScreen;


    [SerializeField] GameObject RetrainingModelScreen;
    [SerializeField] GameObject RetrainingModelText;
    [SerializeField] GameObject RetrainingSuccededText;
    [SerializeField] GameObject RetrainingFailedText;
    bool modelretrained = false;

    public GameObject RecordingSamplesScreen;
    public GameObject RecordingFinishedScreen;

    public TMP_Text GuestureName;

    public static AppManager Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void SamplesRecorded()
    {
        RecordingFinishedScreen.SetActive(true);
        RecordingSamplesScreen.SetActive(false);
    }

    public void CurrentGuestureDectected(string name)
    {
        GuestureName.text = name;
    }

    public void RetrainingModel()
    {
        HandTrackingScreen.SetActive(false);
        RetrainingModelScreen.SetActive(true);
   
        StartCoroutine(RetrainModel());
    }

    public void ModelRetrained()
    {
        modelretrained = true;
    }

    IEnumerator RetrainModel()
    {
        int i = 0;
        while(modelretrained==false && i<4)
        {
            yield return new WaitForSecondsRealtime(5);
            i++;
        }
        RetrainingFailedText.SetActive(!modelretrained);
        RetrainingModelText.SetActive(false);
        RetrainingSuccededText.SetActive(modelretrained);
        yield return new WaitForSecondsRealtime(3);
        HandTrackingScreen.SetActive(true);
        RetrainingModelScreen.SetActive(false);
        RetrainingModelText.SetActive(true);
        RetrainingSuccededText.SetActive(false);
        RetrainingFailedText.SetActive(false);
    }
}

