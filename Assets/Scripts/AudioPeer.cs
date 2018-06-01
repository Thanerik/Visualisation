using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour {
    // Constants defining the numbers
    private const int numFreqBands = 11;
    private const int numberOfSamples = 1024;
    private int outputSampleRate;

    private AudioSource audioSource;

    // File input
    public AudioClip AudioClip;
    
    // Microphone input
    public bool UseMicrophone;
    public AudioMixerGroup MixerGroupMicrophone, MixerGroupMaster;

    // Audio profile
    public float AudioProfile;

    // FFT values:
    private float[] samplesLeft;
    private float[] samplesRight;
    [HideInInspector] public float[] SamplesStero;

    private float[] freqBand = new float[numFreqBands];
    private float[] bandBuffer = new float[numFreqBands];
    private float[] bufferDecrease = new float[numFreqBands];
    private float[] freqBandHighest = new float[numFreqBands];

    // Audio band values (Public to other scripts)
    [HideInInspector] public float[] AudioBand, AudioBandBuffer;
    //public float[] AudioBand, AudioBandBuffer;

    // Amplitude values (Public to other scripts)
    [HideInInspector] public float Amplitude, AmplitudeBuffer;

    // Mono or Stero
    public static bool IsStero;

    [Space(15)] public string ChosenMicrophone;
    public float AmplitudeLeft, AmplitudeRight;

    private float amplitudeHighest;

    // Use this for initialization
    void Start() {
        outputSampleRate = AudioSettings.outputSampleRate;
        audioSource = GetComponent<AudioSource>();

        samplesLeft = new float[numberOfSamples];
        samplesRight = new float[numberOfSamples];
        SamplesStero = new float[numberOfSamples];

        AudioBand = new float[numFreqBands];
        AudioBandBuffer = new float[numFreqBands];

        SetAudioProfile(AudioProfile);

        if ( UseMicrophone ) {
            ChosenMicrophone = "DUO-CAPTURE";
            //ChosenMicrophone = Microphone.devices[0];
            ConnectMicrophone();
        }
        else { ConnectClip(); }
        
        Debug.Log("Name: "+audioSource.clip.name);
        Debug.Log("ID: "+audioSource.clip.GetInstanceID());
        Debug.Log("Length: "+audioSource.clip.length);
        Debug.Log("Samples: "+audioSource.clip.samples);
        Debug.Log("Channels: "+audioSource.clip.channels);
        Debug.Log("Frequency: "+audioSource.clip.frequency);
        Debug.Log("Ambisonic: "+audioSource.clip.ambisonic);
    }
    
    private void SetAudioProfile(float audioProfile) {
        for (int i = 0; i < numFreqBands; i++) {
            freqBandHighest[i] = audioProfile;
        }
    }

    // Update is called once per frame
    void Update() {      
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        MakeBandBuffer();
        CreateAudioBands();
        CreateAmplitudes();
        //FindDirectionOfAudio();
    }

    private void GetSpectrumAudioSource() {
        audioSource.GetSpectrumData(samplesLeft, 0, FFTWindow.Blackman);
        audioSource.GetSpectrumData(samplesRight, 1, FFTWindow.Blackman);
        Debug.Log(samplesLeft[500]);
        Debug.Log(samplesRight[500]);
        for (int i = 0; i < numberOfSamples; i++) {
            SamplesStero[i] = samplesLeft[i] + samplesRight[i];
        }
    }

    private void MakeFrequencyBands() {
        /*
         * Song / Microphone: 44100 Hz sampling rate
         * 1024 spectrum samples (window size)
         * 44100 / 1024 = 43,06640625 Hz per sample = 23,22 ms analysis window
         */

        // Creates 11 bands - according to Otave Bands
        // Source: https://en.wikipedia.org/wiki/Octave_band
        // TODO: Move it up to "setup" in a array - so we don't need to compute the same value in each frame 
        for (int i = -6; i < 5; i++) {
            float fcenter = Mathf.Pow(10, 3) * Mathf.Pow(2, i);
            float fd = Mathf.Pow(2, 0.5f);
            float fupper = fcenter * fd;
            float flower = fcenter / fd;

            freqBand[i + 6] = BandVol(flower, fupper);
        }
    }

    public void ConnectMicrophone() {
        audioSource.outputAudioMixerGroup = MixerGroupMicrophone;
        audioSource.clip = Microphone.Start(ChosenMicrophone, true, 10, outputSampleRate);
        audioSource.loop = true;
        
        while ( !( Microphone.GetPosition(null) > 0 ) ) { }
        
        SteroCheck();
        audioSource.Play();
        
    }
    
    public void ConnectClip() {
        audioSource.outputAudioMixerGroup = MixerGroupMaster;
        audioSource.clip = AudioClip;
        
        SteroCheck();
        audioSource.Play();
    }

    private float BandVol(float fLow, float fHigh) {
        fLow = Mathf.Clamp(fLow, 20, outputSampleRate); // limit low...
        fHigh = Mathf.Clamp(fHigh, fLow, outputSampleRate); // and high frequencies

        // get spectrum
        int from = Mathf.FloorToInt(fLow * numberOfSamples / outputSampleRate);
        int to = Mathf.FloorToInt(fHigh * numberOfSamples / outputSampleRate);
        float sum = 0;
        // average the volumes of frequencies fLow to fHigh
        for (var i = from; i <= to; i++) {
            sum += SamplesStero[i];
        }

        return sum / ( to - from + 1 );
    }

    private void MakeBandBuffer() {
        for (int i = 0; i < numFreqBands; i++) {
            if ( freqBand[i] > bandBuffer[i] ) {
                bandBuffer[i] = freqBand[i];
                bufferDecrease[i] = 0.005f;
            }

            if ( freqBand[i] < bandBuffer[i] ) {
                bufferDecrease[i] = ( bandBuffer[i] - freqBand[i] ) / 8; // Increase by 20%
                bandBuffer[i] = bandBuffer[i] - bufferDecrease[i];
            }
        }
    }

    private void CreateAudioBands() {
        for (int i = 0; i < numFreqBands; i++) {
            freqBand[i] = freqBand[i];
            bandBuffer[i] = bandBuffer[i];
            if ( freqBand[i] > freqBandHighest[i] ) {
                freqBandHighest[i] = freqBand[i];
            }

            AudioBand[i] = freqBand[i] / freqBandHighest[i];
            AudioBandBuffer[i] = bandBuffer[i] / freqBandHighest[i];
        }
    }

    private void CreateAmplitudes() {
        Amplitude = GetAmplitude(AudioBand);
        AmplitudeBuffer = GetAmplitude(AudioBandBuffer);
        AmplitudeLeft = GetAmplitude(samplesLeft);
        AmplitudeRight = GetAmplitude(samplesRight);
    }

    private float GetAmplitude(float[] sampleArray) {
        float curAmplitude = 0;

        foreach (var sample in sampleArray) {
            curAmplitude += sample;
        }

        if ( curAmplitude > amplitudeHighest ) {
            amplitudeHighest = curAmplitude;
        }

        return curAmplitude / amplitudeHighest;
    }

    /*
    private Vector2 FindDirectionOfAudio() {
        var left = FloatToComplex(samplesLeft);
        var right = FloatToComplex(samplesRight);
        alglib.complex[] correlation = new alglib.complex[2 * numberOfSamples - 1];
        alglib.corr.corrc1d(left, numberOfSamples, right, numberOfSamples, ref correlation);
        alglib.fftc1dinv(ref correlation);
        double max = 0;
        foreach (alglib.complex argComplex in correlation) {
            double thisComplex = alglib.math.abscomplex(argComplex);
            if ( max < thisComplex ) {
                max = thisComplex;
            }
        }

        return new Vector2();
    }
    */

    public List<string> GetMicrophones() {
        return Microphone.devices.ToList();
    }

    public string GetMicrophone(int index) {
        return Microphone.devices[index];
    }

    public void SteroCheck() {
        IsStero = audioSource.clip.channels > 1;
    }
}