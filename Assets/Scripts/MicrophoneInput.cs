using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour {
    public string MicrophoneName;
    private AudioSource ac;
    public AudioMixerGroup MixerGroupMicrophone;

    // Use this for initialization
    void Start() {
        ac = GetComponent<AudioSource>();

        if ( MicrophoneName != "" ) {
            ac.outputAudioMixerGroup = MixerGroupMicrophone;

            ac.clip = Microphone.Start(MicrophoneName, true, 10, AudioSettings.outputSampleRate);
            ac.loop = true;
            ac.mute = true;

            while ( !( Microphone.GetPosition(null) > 0 ) ) { }

            ac.Play();
        }
    }

    public AudioClip GetAudioClip() {
        return ac.clip;
    }

// Update is called once per frame
    void Update() { }
}