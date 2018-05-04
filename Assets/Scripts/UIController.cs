using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour {

	public void GoTo(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}
