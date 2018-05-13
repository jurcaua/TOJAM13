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
    public Color selectedColor = Color.white;

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
    public Color[] playerColors;
    public Color inGameColor;
    public Color notInGameColor;
    public GameObject[] textPrompts;

    private PlayerUIObject[] playerUI;
    private bool[] selectedColors;

    private int nextPlayerToInit = 0;
    private List<ControlSetting> playerControls;
    private bool keyboardPlayerFound = false;
    private int keyboardPlayerIndex = -1;
    private bool[] controllerIndexFound = { false, false, false, false };
    private int[] controllerPlayerIndicies = { -1, -1, -1, -1 };

    [Header("Game UI")]
    public bool GameMode = false;
    public GameObject pauseMenu;

    [HideInInspector] public bool paused = false;

    private int numPlayers = 1;
    private List<ControlType> controlSchemes;
    private List<bool> isSet;

    private AudioController audioC;

    private class PlayerUIObject {

        public TextMeshProUGUI playerText;
        public TextMeshProUGUI controlType;
        public GameObject playerIn;
        public GameObject playerNotIn;

        public GameObject colorSelectedEdge;
        public Animator colorSelectedEdgeAnim;
        public Image colorSelectedImage;

        public PlayerUIObject(Transform player) {
            playerText = player.Find("PlayerText").GetComponent<TextMeshProUGUI>();
            controlType = playerText.transform.Find("ControlType").GetComponent<TextMeshProUGUI>();
            playerIn = player.Find("PlayerIn").gameObject;
            playerNotIn = player.Find("PlayerNotIn").gameObject;

            colorSelectedEdge = player.Find("ColorSelectedEdge").gameObject;
            colorSelectedEdgeAnim = colorSelectedEdge.GetComponent<Animator>();
            colorSelectedImage = colorSelectedEdge.transform.Find("ColorSelected").GetComponent<Image>();
        }
    }

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

            selectedColors = new bool[playerColors.Length];
            for (int i = 0; i < selectedColors.Length; i++) {
                selectedColors[i] = false;
            }

            playerUI = new PlayerUIObject[] {
                new PlayerUIObject(playerUIObjects[0].transform),
                new PlayerUIObject(playerUIObjects[1].transform),
                new PlayerUIObject(playerUIObjects[2].transform),
                new PlayerUIObject(playerUIObjects[3].transform)
            };

            for (int i = 0; i < playerUI.Length; i++) {
                playerUI[i].colorSelectedImage.color = Color.white;
            }
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

            if (Input.GetKeyDown(KeyCode.X) && keyboardPlayerFound) {
                playerUI[keyboardPlayerIndex].colorSelectedImage.color = GetNextFreeColor(playerUI[keyboardPlayerIndex].colorSelectedImage.color);

            } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Any)) {
                if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.One) && controllerIndexFound[0]) {
                    playerUI[controllerPlayerIndicies[0]].colorSelectedImage.color = GetNextFreeColor(playerUI[controllerPlayerIndicies[0]].colorSelectedImage.color);

                } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Two) && controllerIndexFound[1]) {
                    playerUI[controllerPlayerIndicies[1]].colorSelectedImage.color = GetNextFreeColor(playerUI[controllerPlayerIndicies[1]].colorSelectedImage.color);

                } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Three) && controllerIndexFound[2]) {
                    playerUI[controllerPlayerIndicies[2]].colorSelectedImage.color = GetNextFreeColor(playerUI[controllerPlayerIndicies[2]].colorSelectedImage.color);

                } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Four) && controllerIndexFound[3]) {
                    playerUI[controllerPlayerIndicies[3]].colorSelectedImage.color = GetNextFreeColor(playerUI[controllerPlayerIndicies[3]].colorSelectedImage.color);
                }
            }

            if (nextPlayerToInit < 4) {

                if (Input.GetKeyDown(KeyCode.X) && !keyboardPlayerFound) {
                    playerUI[nextPlayerToInit].playerText.color = inGameColor; // set color to green (in game)
                    playerUI[nextPlayerToInit].playerNotIn.SetActive(false); // disable crying image
                    playerUI[nextPlayerToInit].playerIn.SetActive(true); // enable dancing image
                    playerUI[nextPlayerToInit].colorSelectedEdgeAnim.SetTrigger("Waiting"); // set animation to prompt for color select
                    playerUI[nextPlayerToInit].colorSelectedImage.color = GetNextFreeColor();
                    playerUI[nextPlayerToInit].controlType.text = "Keyboard"; // set control type found
                    playerControls[nextPlayerToInit] = new ControlSetting(ControlType.Keyboard); // update control type in backend

                    keyboardPlayerIndex = nextPlayerToInit;

                    nextPlayerToInit++;

                    keyboardPlayerFound = true;
                    canContinue = true;
                } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Any)) {
                    ControlSetting newControl = new ControlSetting(ControlType.Controller);
                    if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.One) && !controllerIndexFound[0]) {
                        newControl.controllerIndex = GamePad.Index.One;
                        controllerIndexFound[0] = true;
                        controllerPlayerIndicies[0] = nextPlayerToInit;

                    } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Two) && !controllerIndexFound[1]) {
                        newControl.controllerIndex = GamePad.Index.Two;
                        controllerIndexFound[1] = true;
                        controllerPlayerIndicies[1] = nextPlayerToInit;

                    } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Three) && !controllerIndexFound[2]) {
                        newControl.controllerIndex = GamePad.Index.Three;
                        controllerIndexFound[2] = true;
                        controllerPlayerIndicies[2] = nextPlayerToInit;

                    } else if (GamePad.GetButtonDown(GamePad.Button.X, GamePad.Index.Four) && !controllerIndexFound[3]) {
                        newControl.controllerIndex = GamePad.Index.Four;
                        controllerIndexFound[3] = true;
                        controllerPlayerIndicies[3] = nextPlayerToInit;
                    }

                    if (newControl.controllerIndex != GamePad.Index.Any) {
                        playerUI[nextPlayerToInit].playerText.color = inGameColor; // set color to green (in game)
                        playerUI[nextPlayerToInit].playerNotIn.SetActive(false); // disable crying image
                        playerUI[nextPlayerToInit].playerIn.SetActive(true); // enable dancing image
                        playerUI[nextPlayerToInit].colorSelectedEdgeAnim.SetTrigger("Waiting"); // set animation to prompt for color select
                        playerUI[nextPlayerToInit].colorSelectedImage.color = GetNextFreeColor();
                        playerUI[nextPlayerToInit].controlType.text = "Keyboard"; // set control type found
                        playerControls[nextPlayerToInit] = newControl; // update control type in backend

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

    private Color GetNextFreeColor() {
        if (selectedColors.Length <= 0) {
            return Color.black;
        }
        for (int i = 0; i < selectedColors.Length; i++) {
            if (!selectedColors[i]) {
                selectedColors[i] = true;
                return playerColors[i];
            }
        }
        return Color.black;
    }

    private Color GetNextFreeColor(Color afterThis) {
        if (selectedColors.Length <= 0) {
            return Color.black;
        }
        // find index of afterThis
        int afterThisIndex = -1;
        for (int i = 0; i < playerColors.Length; i++) {
            if (playerColors[i] == afterThis) {
                afterThisIndex = i;
            }
        }
        if (afterThisIndex == -1) {
            return Color.black;
        }

        // get the next useable color afterwards
        for (int i = 0; i < selectedColors.Length; i++) {
            int curIndex = (afterThisIndex + i) % selectedColors.Length;
            if (!selectedColors[curIndex]) {
                selectedColors[afterThisIndex] = false;
                selectedColors[curIndex] = true;
                return playerColors[curIndex];
            }
        }
        return playerColors[afterThisIndex];
    }

    public void GoTo(string sceneName) {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }

    public void GoToWait(string sceneName) {
        StartCoroutine(WaitLoad(sceneName, audioC.PlayBigUIClick() / 3));
    }

    public void GoToShortWait(string sceneName) {
        StartCoroutine(WaitLoad(sceneName, audioC.PlayerSmallUIClick()));
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

            for (int i = 0; i < SettingManager.NumberOfPlayers; i++) {
                playerControls[i].selectedColor = playerUI[i].colorSelectedImage.color;
            }
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

    public void RestartScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            Debug.Log(string.Format("Player:{0}, Setting={1}", 
                (i + 1).ToString(), 
                (playerControls[i].controllerIndex != GamePad.Index.Any) ? "Controller " + playerControls[i].controllerIndex.ToString() : "Keyboard")
            );
        }
        /*
        for (int i = 0; i < numPlayers; i++) {
            Debug.Log(string.Format("Player:{0}, Setting={1}", (i + 1).ToString(), controlSchemes[i]));
        }
        */
    }
}
