using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour {

	public float speed;
	public float h;
	public float v;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.A) ){
			transform.position -= Vector3.right * speed;
		}
		if (Input.GetKey(KeyCode.D) ){
			transform.position += Vector3.right * speed;
		}
	}
}
