using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using EventTrigger = UnityEngine.EventSystems.EventTrigger;

public class HatAmplitude : MonoBehaviour {

	public GameObject HatPrefab;

	private GameObject hat;
	private Renderer hatRenderer;
	
	// Use this for initialization
	void Start () {
		GameObject hatObject = Instantiate(HatPrefab);
		hatObject.transform.position = transform.position + new Vector3(0, 0.3f, 0);
		hatObject.name = "Hat" + name.Substring(name.Length - 3);

		hat = hatObject;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 cubeUpper = GetComponent<Renderer>().bounds.max;
		if ( hat != null ) {
			Vector3 hatPosition = hat.transform.position;
			//Debug.Log("Cube Upper: "+cubeUpper+" and Hat: "+hatPosition);
			if ( cubeUpper.y >= hatPosition.y ) {
				hat.GetComponent<Rigidbody>().MovePosition(new Vector3(0, cubeUpper.y, 0));
			}
		}
	}
}
