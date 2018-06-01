using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideLightIntensity : MonoBehaviour {
    private Light thisLight;
    public AudioPeer AudioPeer;

    public enum Side {
        Right,
        Left
    }
    public Side WhichSide;
    public int IntensityMultiplier;
    public float IntensityStart;

// Use this for initialization
    void Start() {
        thisLight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update() {
        switch ( WhichSide ) {
            case Side.Left:
                thisLight.intensity = AudioPeer.AmplitudeLeft * IntensityMultiplier + IntensityStart;
                break;
            case Side.Right:
                thisLight.intensity = AudioPeer.AmplitudeRight * IntensityMultiplier + IntensityStart;
                break;
        }
    }
}