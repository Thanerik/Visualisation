using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioInput : MonoBehaviour {
    private const bool debug = true;

    private AudioSource audioSource;

    public int ClipLength = 10;
    public int ClipSample = 44100;
    public int SampleDataLength = 1024;
    public float UpdateStep = 0.1f; // 100ms
    
    private float[] clipSampleDataRight;
    private float[] clipSampleDataLeft;
    private float[] spectrumData;

    private float currentUpdateTime;
    private int numChannels;

    public Text LoudnessText;
    public Text RightLoudness;
    public Text LeftLoudness;

    //AudioClip ac = new AudioClip();
    private string myMicrophone;

    private void Awake() {
        audioSource = GetComponent<AudioSource>(); // Get this component
        if ( !audioSource ) {
            Debug.LogError(GetType() + ".Awake: There was no audioSoucrce set.");
        }

        if ( Microphone.devices.Length == 0 ) {
            Debug.LogError(GetType() + ".Awake: We can't find any microphones devices");
        }

        myMicrophone = Microphone.devices[0];
        if ( debug ) {
            Debug.Log("Microphone: " + myMicrophone);
        }

        clipSampleDataRight = new float[SampleDataLength];
        clipSampleDataLeft = new float[SampleDataLength];
        spectrumData = new float[SampleDataLength];
    }

    // Use this for initialization
    void Start() {
        audioSource.clip = Microphone.Start(myMicrophone, true, ClipLength, ClipSample);
        audioSource.loop = true;
        while (!(Microphone.GetPosition(null) > 0)){}
        audioSource.Play();
        
        numChannels = audioSource.clip.channels;

        if ( !MultipleChannels() ) {
            Debug.Log("WARNING! THIS AUDIO IS IN MONO!");
        }
    }

    // Update is called once per frame
    void Update() {
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);
        //AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);

        currentUpdateTime += Time.deltaTime;
        if ( currentUpdateTime >= UpdateStep ) {
            currentUpdateTime = 0f;
            
            if ( MultipleChannels() ) {
                audioSource.GetOutputData(clipSampleDataLeft, 1);
                WriteLoudness(clipSampleDataLeft, LeftLoudness);
            }
            else {
                LoudnessText.text = "Mono:";
                LeftLoudness.text = "";
            }
            
            audioSource.GetOutputData(clipSampleDataRight, 0);
            WriteLoudness(clipSampleDataRight, RightLoudness);
            
        }
    }
    
    private bool MultipleChannels() {
        return numChannels > 1;
    }

    private void WriteLoudness(float[] samples, Text text) {
        float loudness = 0f;
        foreach (float sample in clipSampleDataRight) {
            loudness += Mathf.Abs(sample) / SampleDataLength;
        }
        text.text = loudness.ToString();
    }
}