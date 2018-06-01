using UnityEngine;

public class SetupPlayerPrefs : MonoBehaviour {
    private const string ssVar = "ScreenShotCounter";
    private int screenShotCounter; 
    
    // Use this for initialization
    void Start() {
        if ( !PlayerPrefs.HasKey(ssVar) ) {
            PlayerPrefs.SetInt(ssVar, screenShotCounter);
        }
        else {
            screenShotCounter = PlayerPrefs.GetInt(ssVar);
        }
    }

    void IncreaseScreenShotCounter() {
        screenShotCounter++;
        PlayerPrefs.SetInt(ssVar, screenShotCounter);
    }
}