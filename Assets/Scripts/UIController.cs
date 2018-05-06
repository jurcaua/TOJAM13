using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public enum ControlType {Keyboard = 0, Controller = 1};

public class UIController : MonoBehaviour {

    [Header("Player Control Setting Scene")]
    public bool SettingMode = true;
    public Image continueButtonImage;
    public TextMeshProUGUI debugModeButtonText;

    private bool canContinue = false;

    private int numPlayers = 1;
    private List<ControlType> controlSchemes;
    private List<bool> isSet;

    private AudioController audio;

    void Start() {
        audio = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>();

        if (SettingMode) {
            ResetControls();
            continueButtonImage.color = new Color(1f, 1f, 1f, 0.5f);
        }
    }

	public void GoTo(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void GoToWait(string sceneName) {
        StartCoroutine(WaitLoad(sceneName, audio.PlayBigUIClick()));
    }

    IEnumerator WaitLoad(string sceneName, float delay) {
        yield return new WaitForSeconds(delay);

        GoTo(sceneName);
    }

    public void SetNumPlayers(int num) {
        numPlayers = num;
        ResetControls();
    }

    public void SetPlayer(PlayerControl toSet) {
        controlSchemes[toSet.playerNum - 1] = toSet.controlType;
        isSet[toSet.playerNum - 1] = true;
        if (toSet.controlType == ControlType.Keyboard) {
            for (int i = 0; i < numPlayers; i++) {
                if (i != toSet.playerNum - 1) {
                    controlSchemes[i] = ControlType.Controller;
                    isSet[i] = true;
                }
            }
        }

        bool allTrue = true;
        for (int i = 0; i < numPlayers; i++) {
            if (!isSet[i]) {
                allTrue = false;
                break;
            }
        }
        
        if (allTrue) {
            canContinue = true;
            continueButtonImage.color = Color.white;
        }

        //PrintCurrentSettings();
    }

    public void ToggleDebugMode() {
        SettingManager.DebugMode = !SettingManager.DebugMode;
        debugModeButtonText.text = "Debug Mode: " + (SettingManager.DebugMode ? "ON" : "OFF");
    }

    public void FinalizeSettings() {
        if (canContinue) {
            SettingManager.HasBeenSetUp = true;
            SettingManager.NumberOfPlayers = numPlayers;
            SettingManager.ControlSchemes = controlSchemes;

            PrintCurrentSettings();

            //GoTo(nextScene);
        }
    }

    void ResetControls() {
        controlSchemes = new List<ControlType>();
        isSet = new List<bool>();
        for (int i = 0; i < numPlayers; i++) {
            controlSchemes.Add(ControlType.Controller);
            isSet.Add(false);
        }
    }

    void PrintCurrentSettings() {
        for (int i = 0; i < numPlayers; i++) {
            Debug.Log(string.Format("Player:{0}, Setting={1}", (i + 1).ToString(), controlSchemes[i]));
        }
    }
}
