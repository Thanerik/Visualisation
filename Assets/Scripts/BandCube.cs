using UnityEngine;

public class BandCube : MonoBehaviour {
    public AudioPeer AudioPeer;
    public int Band;
    public float StartScale, ScaleMultiplier;
    public bool UseBuffer;

    private Renderer theRender;
    //private ParticleSystem particleSystem;

    private const float startHue = 80.0f;
    private const float endHue = 0.1f;

    private Color currentColor;
    private Color currentEmission;

    // Use this for initialization
    void Start() {
        theRender = GetComponent<Renderer>();
        //particleSystem = GetComponent<ParticleSystem>();
        //particleSystem.Stop();
        theRender.material.color = Color.HSVToRGB(startHue, 255, 255);
    }

    // Update is called once per frame
    void Update() {
        float reference = !UseBuffer ? AudioPeer.AudioBand[Band] : AudioPeer.AudioBandBuffer[Band];

        transform.localScale = new Vector3(transform.localScale.x,
            reference * ScaleMultiplier + StartScale, transform.localScale.z);

        float hue = GetHue(reference);
        float emission = GetEmission(reference);

        currentColor = Color.HSVToRGB(hue, 1, 1);
        currentEmission = Color.HSVToRGB(hue, 1, emission);

        theRender.material.SetColor("_Color", currentColor);
        theRender.material.SetColor("_EmissionColor", currentEmission);

        /*
        if ( reference > 0.8f ) {
            particleSystem.Play();
        }
        else {
            particleSystem.Stop();
        }
        */
    }

    private float GetEmission(float value) {
        return Mathf.Clamp(value * 0.5f, 0.2f, 0.5f);
    }

    private float GetHue(float value) {
        return ( startHue - startHue * value + endHue ) / 360;
    }

    private void OnCollisionEnter(Collision other) {
        other.collider.GetComponent<Renderer>().material.SetColor("_Color", currentColor);
        other.collider.GetComponent<Renderer>().material.SetColor("_EmissionColor", currentEmission);
    }
}