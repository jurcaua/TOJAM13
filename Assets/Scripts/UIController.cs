using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using GamepadInput;

public enum ControlType {Keyboard = 0, Controller = 1};

public class ControlSetting {
    public ControlType controlType;
    public GamePad.Index controllerIndex;
    public bool isSet;

    public ControlSetting (ControlType _controlType, bool _isSet = true, GamePad.Index _controllerIndex = GamePad.Index.Any) {
        controlType = _controlType;
        controllerIndex = _controllerIndex;
        isSet = _isSet;
    }
}

public class UIController : MonoBehaviour {

    [Header("Main Menu UI")]
    public bool IsMainMenu = false;

    [Header("Control Setting UI")]
    public bool SettingMode = true;
    public Image continueButtonImage;
    public TextMeshProUGUI debugModeButtonText;
    public TextMeshProUGUI warningText;

    private bool canContinue = false;

    [Header("Updated Control Setting UI")]
    public bool UpdatedSettingMode = false;
    public GameObject[] playerUIObjects;
    public Color inGameColor;
    public Color notInGameColor;
    public GameObject[] textPrompts;

    private int nextPlayerToInit = 0;
    private List<ControlSetting> playerControls;
    private bool keyboardPlayerFound = false;
    private bool[] controllerIndexFound = { false, false, false, false };

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

        if (UpdatedSettingMode) {
            playerControls = new List<ControlSetting> {
                new ControlSetting(ControlType.Controller, false),
                new ControlSetting(ControlType.Controller, false),
                new ControlSetting(ControlType.Controller, false),
                new ControlSetting(ControlType.Controller, false)
            };
        }

        if (GameMode) {
            pauseMenu.SetActive(paused);
        }
    }

    void Update() {
        if (IsMainMenu) {
            if (Input.GetKeyDown(KeyCode.Escape) || GamePad.GetButtonDown(GamepadInput.GamePad.Button.Start, GamepadInput.GamePad.Index.Any)) {
                Application.Quit();
            }
        }

        if (UpdatedSettingMode) {
            for (int i = 0; i < textPrompts.Length; i++) {
                if (i == nextPlayerToInit) {
                    textPrompts[i].SetActive(true);
                } else {
                    textPrompts[i].SetActive(false);
                }
            }

            if (nextPlayerToInit < 4) {

                if (Input.GetKeyDown(KeyCode.X) && !keyboardPlayerFound) {
                    Transform currentPlayer = playerUIObjects[nextPlayerToInit].transform;
                    currentPlayer.Find("PlayerText").GetComponent<TextMeshProUGUI>().color = inGameColor;
                    currentPlayer.Find("PlayerNotIn").gameObject.SetActive(false);
                    currentPlayer.Find("PlayerIn").gameObject.SetActive(true);
                    currentPlayer.Find("ControlType").GetComponent<TextMeshProUGUI>().text = "Keyboard";
                    playerControls[nextPlayerToInit] = new ControlSetting(ControlType.Keyboard);

                    nextPlayerToInit++;

                    keyboardPlayerFound = true;
                    canContinue = true;
                } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Any)) {
                    ControlSetting newControl = new ControlSetting(ControlType.Controller);
                    if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.One) && !controllerIndexFound[0]) {
                        newControl.controllerIndex = GamePad.Index.One;
                        controllerIndexFound[0] = true;

                    } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Two) && !controllerIndexFound[1]) {
                        newControl.controllerIndex = GamePad.Index.Two;
                        controllerIndexFound[1] = true;

                    } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Three) && !controllerIndexFound[2]) {
                        newControl.controllerIndex = GamePad.Index.Three;
                        controllerIndexFound[2] = true;

                    } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Four) && !controllerIndexFound[3]) {
                        newControl.controllerIndex = GamePad.Index.Four;
                        controllerIndexFound[3] = true;
                    }

                    if (newControl.controllerIndex != GamePad.Index.Any) {
                        Transform currentPlayer = playerUIObjects[nextPlayerToInit].transform;
                        currentPlayer.Find("PlayerText").GetComponent<TextMeshProUGUI>().color = inGameColor;
                        currentPlayer.Find("PlayerNotIn").gameObject.SetActive(false);
                        currentPlayer.Find("PlayerIn").gameObject.SetActive(true);
                        currentPlayer.Find("ControlType").GetComponent<TextMeshProUGUI>().text = "Controller";
                        playerControls[nextPlayerToInit] = newControl;

                        nextPlayerToInit++;

                        canContinue = true;
                    }
                }
            }
        }

        if (GameMode) {
            if (Input.GetKeyDown(KeyCode.Escape) || GamepadInput.GamePad.GetButtonDown(GamepadInput.GamePad.Button.Start, GamepadInput.GamePad.Index.Any)) {
                pauseMenu.SetActive(!paused);
                paused = !paused;
                Time.timeScale = 1 - Time.timeScale;
            }

            if (paused && GamepadInput.GamePad.GetButtonDown(GamepadInput.GamePad.Button.A, GamepadInput.GamePad.Index.Any)) {
                paused = false;
                Time.timeScale = 1 - Time.timeScale;
                GoTo("main-menu");
            }
        }
    }

	public void GoTo(string sceneName) {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }

    public void GoToWait(string sceneName) {
        StartCoroutine(WaitLoad(sceneName, audioC.PlayBigUIClick() / 3));
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
        if (UpdatedSettingMode & canContinue) {
            SettingManager.HasBeenSetUp = true;
            SettingManager.NumberOfPlayers = nextPlayerToInit;
            SettingManager.PlayerControls = playerControls;

            PrintCurrentSettings();

            //GoTo(nextScene);
            GoToWait(sceneName);
        }
        return;
        /*
        if (canContinue) {
            SettingManager.FindControllers();
            SettingManager.HasBeenSetUp = true;
            SettingManager.NumberOfPlayers = numPlayers;
            SettingManager.ControlSchemes = controlSchemes;

            PrintCurrentSettings();

            //GoTo(nextScene);
            GoToWait(sceneName);
        }
        */
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
        for (int i = 0; i < nextPlayerToInit; i++) {
            Debug.Log(string.Format("Player:{0}, Setting={1}", (i + 1).ToString(), playerControls[i].controllerIndex));
        }
        /*
        for (int i = 0; i < numPlayers; i++) {
            Debug.Log(string.Format("Player:{0}, Setting={1}", (i + 1).ToString(), controlSchemes[i]));
        }
        */
    }
}
