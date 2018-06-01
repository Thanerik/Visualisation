using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiate512Cubes : MonoBehaviour {
	public AudioPeer AudioPeer;
	public GameObject CubePrefab;
	private int numberOfCubes;
	public float MaxScale;
	
	GameObject[] cubes;
	
	// Use this for initialization
	void Start () {
		numberOfCubes = AudioPeer.SamplesStero.Length;
		cubes = new GameObject[numberOfCubes];

		for (int i = 0; i < numberOfCubes; i++) {
			GameObject instanceCube = Instantiate(CubePrefab);
			instanceCube.transform.position = transform.position;
			instanceCube.transform.parent = transform;
			instanceCube.name = "SampleCube" + i;
			
			transform.eulerAngles = new Vector3(0, -(numberOfCubes/360) * i, 0);
			instanceCube.transform.position = Vector3.forward * 1000;

			cubes[i] = instanceCube;
		}
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < numberOfCubes; i++) {
			if ( cubes[i] != null ) {
				cubes[i].transform.localScale = new Vector3(10,AudioPeer.SamplesStero[i] * MaxScale + 2,10);
			}
		}
	}
}
