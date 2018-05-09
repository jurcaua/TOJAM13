using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seagull : MonoBehaviour {

    private UIController uiController;

	// Use this for initialization
	void Start () {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
    }
	
	// Update is called once per frame
	void Update () {
        if (uiController != null && !uiController.paused) {
            transform.position += transform.right * 0.05f;
        }
	}

	//TODO after hooked, dissapear in an explosion of feathers
}
