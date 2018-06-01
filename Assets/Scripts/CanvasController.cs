using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour {
    private const string UIVar = "UserInterface";

    public AudioPeer AudioPeer;

    private Canvas infoCanvas;

    public Text LoudnessText;
    public Text RightLoudness;
    public Text LeftLoudness;

    public Dropdown Dropdown;
    public Toggle Toggle;
    public Slider Slider;

    // Use this for initialization
    void Start() {
        if ( !PlayerPrefs.HasKey(UIVar) ) {
            SetBool(UIVar, true);
        }

        infoCanvas = GetComponent<Canvas>();
        infoCanvas.enabled = GetBool(UIVar);

        Dropdown.ClearOptions();
        Dropdown.AddOptions(AudioPeer.GetMicrophones());
        Dropdown.onValueChanged.AddListener(delegate { ChangeMicrophone(Dropdown); });

        Toggle.isOn = AudioPeer.UseMicrophone;
        Toggle.onValueChanged.AddListener(delegate(bool check) {
            AudioPeer.UseMicrophone = check;
            if ( check ) {
                AudioPeer.ConnectMicrophone();
            }

            if ( !check ) {
                AudioPeer.ConnectClip();
            }

            //AudioPeer.SteroCheck();
        });
    }

    // Update is called once per frame
    void Update() {
        if ( Input.GetKeyDown(KeyCode.Space) ) {
            bool current = GetBool(UIVar);
            infoCanvas.enabled = !current;
            SetBool(UIVar, !current);
        }

        LeftLoudness.text = AudioPeer.AmplitudeLeft.ToString();
        if ( AudioPeer.IsStero ) {
            LoudnessText.text = "Left:\nRight:";
            RightLoudness.text = AudioPeer.AmplitudeRight.ToString();
        }
        else {
            LoudnessText.text = "Mono:";
            RightLoudness.text = "";
        }
    }

    private void ChangeMicrophone(Dropdown change) {
        AudioPeer.ChosenMicrophone = AudioPeer.GetMicrophone(change.value);
        AudioPeer.ConnectMicrophone();
    }

    private void SetBool(string key, bool state) {
        PlayerPrefs.SetInt(key, state ? 1 : 0);
    }

    private bool GetBool(string key) {
        return PlayerPrefs.GetInt(key) == 1;
    }

    private bool GetBool(string key, bool defaultValue) {
        return PlayerPrefs.HasKey(key) ? GetBool(key) : defaultValue;
    }
}