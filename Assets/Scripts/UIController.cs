using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public enum ControlType {Keyboard = 0, Controller = 1};

public class UIController : MonoBehaviour {

    [Header("Main Menu UI")]
    public bool IsMainMenu = false;

    [Header("Control Setting UI")]
    public bool SettingMode = true;
    public Image continueButtonImage;
    public TextMeshProUGUI debugModeButtonText;
    public TextMeshProUGUI warningText;

    private bool canContinue = false;

    [Header("Game UI")]
    public bool GameMode = false;
    public GameObject pauseMenu;

    [HideInInspector] public bool paused = false;

    private int numPlayers = 1;
    private List<ControlType> controlSchemes;
    private List<bool> isSet;

    private AudioController audioC;

    void Start() {
        audioC = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>();

        if (SettingMode) {
            ResetControls();
            continueButtonImage.color = new Color(1f, 1f, 1f, 0.5f);
        }

        if (GameMode) {
            pauseMenu.SetActive(paused);
        }
    }

    void Update() {
        if (IsMainMenu) {
            if (Input.GetKeyDown(KeyCode.Escape) || GamepadInput.GamePad.GetButtonDown(GamepadInput.GamePad.Button.Start, GamepadInput.GamePad.Index.Any)) {
                Application.Quit();
            }
        }

        if (GameMode) {
            if (Input.GetKeyDown(KeyCode.Escape) || GamepadInput.GamePad.GetButtonDown(GamepadInput.GamePad.Button.Start, GamepadInput.GamePad.Index.Any)) {
                pauseMenu.SetActive(!paused);
                paused = !paused;
                Time.timeScale = 1 - Time.timeScale;
            }

            if (paused && GamepadInput.GamePad.GetButtonDown(GamepadInput.GamePad.Button.A, GamepadInput.GamePad.Index.Any)) {
                GoTo("main-menu");
            }
        }
    }

	public void GoTo(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void GoToWait(string sceneName) {
        StartCoroutine(WaitLoad(sceneName, audioC.PlayBigUIClick()));
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
        canContinue = false;

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

        int connectedControllers = SettingManager.FindControllers();
        int selectedControllers = 0;
        for (int i = 0; i < numPlayers; i++) {
            if (controlSchemes[i] == ControlType.Controller) {
                selectedControllers++;
            }
        }
        if (selectedControllers > connectedControllers) {
            allTrue = false;
            warningText.text = "Not enough controllers connected!";
        } else {
            warningText.text = "Configure Settings";
        }

        if (allTrue) {
            canContinue = true;
            continueButtonImage.color = Color.white;
        } else {
            canContinue = false;
            continueButtonImage.color = new Color(1f, 1f, 1f, 0.5f);
        }

        //PrintCurrentSettings();
    }

    public void ToggleDebugMode() {
        SettingManager.DebugMode = !SettingManager.DebugMode;
        debugModeButtonText.text = "Debug Mode: " + (SettingManager.DebugMode ? "ON" : "OFF");
    }

    public void FinalizeSettings(string sceneName) {
        if (canContinue) {
            SettingManager.FindControllers();
            SettingManager.HasBeenSetUp = true;
            SettingManager.NumberOfPlayers = numPlayers;
            SettingManager.ControlSchemes = controlSchemes;

            PrintCurrentSettings();

            //GoTo(nextScene);
            GoToWait(sceneName);
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
