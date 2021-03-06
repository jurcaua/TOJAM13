﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;

    public List<GameObject> players;
	public List<int> scores;
    public List<TextMeshProUGUI> texts;
    public List<Slider> sliders;

	public List<Material> colors;

    public GameObject playerPrefab;

    private int curHighestPlayerIndex = 0;
    private LineRenderer highestPlayerLine;

    [Header("Camera Following")]
    public CinemachineTargetGroup cameraTargetGroup;
    private List<CinemachineTargetGroup.Target> cameraTargets;
    public float playerTargetRadius = 4f;
    public float playerTargetWeight = 1f;
    public float hookTargetRadius = 2f;
    public float hookTargetWeight = 0.5f;

    [Header("Spawn Points")]
    public GameObject[] boatSpawnPoints;
    public GameObject[] stormSpawnPoints;
    public GameObject[] icebergSpawnPoints;

    [Header("Level Boundaries")]
    public GameObject[] boatBoundaries;
    public GameObject[] stormBoundaries;
    public GameObject[] icebergBoundaries;

    [Header("Timer and Game Over")]
    public TextMeshProUGUI timerText;
    private int currentTime;

    public GameObject winnerTextObject;
    private Animator winnerTextAnim;

    private StageManager stageManager;
    private UIController uiController;
    
    void Awake() {

        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        highestPlayerLine = GetComponent<LineRenderer>();
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        uiController = GameObject.Find("UIController").GetComponent<UIController>();

        currentTime = stageManager.gameDuration;
        StartCoroutine(Timer());
        
        winnerTextAnim = winnerTextObject.GetComponent<Animator>();
        winnerTextObject.SetActive(false);

        players = new List<GameObject>();
        scores = new List<int>();

        if (SettingManager.HasBeenSetUp) {
            LoadPlayersFromSettings();
        } else {
            LoadPlayersFromScene();
        }
        InitSpawnPlayers();

        InitCameraFollowing();

        InitPlayerColors();

        SetBoundaries();
    }

    public void SetBoundaries() {
        if (stageManager.state == StageManager.GameState.Boat) {
            SetActiveBoatBoundaries(true);
            SetActiveStormBoundaries(false);
            SetActiveIcebergBoundaries(false);

        } else if (stageManager.state == StageManager.GameState.Storm) {
            SetActiveBoatBoundaries(false);
            SetActiveStormBoundaries(true);
            SetActiveIcebergBoundaries(false);

        } else if (stageManager.state == StageManager.GameState.Iceberg) {
            SetActiveBoatBoundaries(false);
            SetActiveStormBoundaries(false);
            SetActiveIcebergBoundaries(true);

        } else {
            Debug.Log(string.Format("State with name \"{0}\" found...", stageManager.state.ToString()));
        }
    }

    void SetActiveBoatBoundaries(bool active) {
        for (int i = 0; i < boatBoundaries.Length; i++) {
            boatBoundaries[i].SetActive(active);
        }
    }

    void SetActiveStormBoundaries(bool active) {
        for (int i = 0; i < stormBoundaries.Length; i++) {
            stormBoundaries[i].SetActive(active);
        }
    }

    void SetActiveIcebergBoundaries(bool active) {
        for (int i = 0; i < icebergBoundaries.Length; i++) {
            icebergBoundaries[i].SetActive(active);
        }
    }

    void InitSpawnPlayers() {
        int randomStartIndex = Random.Range(0, boatSpawnPoints.Length);
        for (int i = 0; i < players.Count; i++) {
            players[i].transform.position = boatSpawnPoints[randomStartIndex].transform.position;
            randomStartIndex = (randomStartIndex + 1) % boatSpawnPoints.Length;
        }
    }

    public void Respawn(Transform playerToRespawn) {
		playerToRespawn.GetComponent<PlayerMovement> ().grapple.UnGrapple ();
		playerToRespawn.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
		playerToRespawn.GetComponent<PlayerMovement> ().ac.SetTrigger ("Unhook");
		scores [playerToRespawn.GetComponent<PlayerMovement> ().playerID - 1] = scores [playerToRespawn.GetComponent<PlayerMovement> ().playerID - 1] - 5;

		if (playerToRespawn.GetComponent<PlayerMovement> ().hit == true) {
			players [playerToRespawn.GetComponent<PlayerMovement> ().hitBy - 1].GetComponent<PlayerMovement> ().grapple.Reset ();
			players [playerToRespawn.GetComponent<PlayerMovement> ().hitBy - 1].GetComponent<PlayerMovement> ().grapple.StopAllCoroutines ();

			players [playerToRespawn.GetComponent<PlayerMovement> ().hitBy - 1].GetComponent<PlayerMovement>().grapple.UnGrapple();
		}
        if (stageManager.state == StageManager.GameState.Boat) {
            playerToRespawn.position = boatSpawnPoints[Random.Range(0, boatSpawnPoints.Length)].transform.position;
        } else if (stageManager.state == StageManager.GameState.Storm) {
            playerToRespawn.position = stormSpawnPoints[Random.Range(0, stormSpawnPoints.Length)].transform.position;
        } else {
            playerToRespawn.position = icebergSpawnPoints[Random.Range(0, icebergSpawnPoints.Length)].transform.position;
        }
    }

    void InitCameraFollowing() {
        cameraTargets = new List<CinemachineTargetGroup.Target>();

        cameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[players.Count];
        for (int i = 0; i < players.Count; i++) {
            AddCameraPlayerTarget(players[i].transform);
        }
    }

    void InitPlayerColors() {
        if (SettingManager.HasBeenSetUp) {
            for (int i = 0; i < SettingManager.NumberOfPlayers; i++) {
                if (SettingManager.PlayerControls[i].selectedColor != Color.white) {
                    colors[i].color = SettingManager.PlayerControls[i].selectedColor;

                    Color fadedSelectedColor = SettingManager.PlayerControls[i].selectedColor;
                    fadedSelectedColor.a /= 4;
                    sliders[i].gameObject.transform.Find("Background").GetComponent<Image>().color = fadedSelectedColor;

                    sliders[i].gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = SettingManager.PlayerControls[i].selectedColor;
                }
            }
        }
    }

    public void AddCameraPlayerTarget(Transform toAdd) {
        if (!IsTarget(toAdd)) {
            CinemachineTargetGroup.Target curTarget = new CinemachineTargetGroup.Target();
            curTarget.target = toAdd;
            curTarget.weight = playerTargetWeight;
            curTarget.radius = playerTargetRadius;
            cameraTargets.Add(curTarget);
            cameraTargetGroup.m_Targets = cameraTargets.ToArray();
        }
    }

    public void AddCameraHookTarget(Transform toAdd) {
        if (!IsTarget(toAdd)) {
            CinemachineTargetGroup.Target curTarget = new CinemachineTargetGroup.Target();
            curTarget.target = toAdd;
            curTarget.weight = hookTargetWeight;
            curTarget.radius = hookTargetRadius;
            cameraTargets.Add(curTarget);
            cameraTargetGroup.m_Targets = cameraTargets.ToArray();
        }
    }

    public void RemoveCameraTarget(Transform toRemove) {
        for (int i = 0; i < cameraTargets.Count; i++) {
            if (cameraTargets[i].target == toRemove) {
                cameraTargets.RemoveAt(i);
                cameraTargetGroup.m_Targets = cameraTargets.ToArray();
                return;
            }
        }
        //Debug.Log(string.Format("Object with name \"{0}\" not found in camera targets.", toRemove.name));
    }

    public bool IsTarget(Transform toCheck) {
        for (int i = 0; i < cameraTargets.Count; i++) {
            if (cameraTargets[i].target == toCheck) {
                return true;
            }
        }
        return false;
    }

    void LoadPlayersFromScene() {
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");

        SettingManager.NumberOfPlayers = foundPlayers.Length;
        if (SettingManager.NumberOfPlayers >= 1) {

            SettingManager.ControlSchemes.Add(ControlType.Keyboard);

            for (int i = 1; i < SettingManager.NumberOfPlayers; i++) {
                SettingManager.ControlSchemes.Add(ControlType.Controller);
            }
        }
        LoadPlayersFromSettings();
    }

    void LoadPlayersFromSettings() {
        // all current players in the scene
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");

        // destroy them all
        for (int i = 0; i < foundPlayers.Length; i++) {
            Destroy(foundPlayers[i]);
        }

        // create the amount we need
        for (int i = 0; i < SettingManager.NumberOfPlayers; i++) {
            GameObject newPlayer = Instantiate(playerPrefab);
            newPlayer.GetComponent<ExperimentalHook>().PlayerID = i + 1;
            players.Add(newPlayer);

			//add colors
			foreach (LineRenderer lr in newPlayer.GetComponentsInChildren<LineRenderer>()) {
				lr.material = colors [i];
			}
			//newPlayer.GetComponent<SpriteRenderer> ().color = Color.blue;
        }

        // set the names and scores
        for (int i = 0; i < players.Count; i++) {
            players[i].name = "Player" + (i + 1);
            scores.Add(0);
        }
        // deactivate the texts of the non-used players
        for (int i = players.Count; i < 4; i++) {
            texts[i].transform.parent.gameObject.SetActive(false);
        }

        if (SettingManager.DebugMode) {
            for (int i = 1; i < players.Count; i++) {
                players[i].GetComponent<PlayerMovement>().enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update() {

        if (uiController.paused) {
            return;
        }
        
        float maxY = float.MinValue;
        int highestPlayerIndex = -1;
        for (int i = 0; i < players.Count; i++) {
            if (players[i].transform.position.y > maxY) {
                maxY = players[i].transform.position.y;
                highestPlayerIndex = i;
            }
        }

        if (curHighestPlayerIndex != highestPlayerIndex) {
            sliders[highestPlayerIndex].value = 0;
            curHighestPlayerIndex = highestPlayerIndex;
        }

        sliders[highestPlayerIndex].value++;
        if (sliders[highestPlayerIndex].value >= sliders[highestPlayerIndex].maxValue) {
            AddScore(highestPlayerIndex, 1);
            sliders[highestPlayerIndex].value = 0;
        }

        timerText.text = currentTime.ToString() + "s";

        highestPlayerLine.SetPosition(0, new Vector3(-100, players[highestPlayerIndex].transform.position.y, -1f));
        highestPlayerLine.SetPosition(1, new Vector3(100, players[highestPlayerIndex].transform.position.y, -1f));
    }

    IEnumerator Timer() {
        while (currentTime > 0) {
            yield return new WaitForSeconds(1f);
            currentTime--;
        }
        GameOver();
    }

    void GameOver() {
        print("Game Over!");

        int highestScore = int.MinValue;
        int winnerIndex = -1;
        for (int i = 0; i < SettingManager.NumberOfPlayers; i++) {
            if (scores[i] > highestScore) {
                highestScore = scores[i];
                winnerIndex = i;
            }
        }
        winnerTextObject.GetComponent<TextMeshProUGUI>().text = string.Format("P{0} Wins!", winnerIndex + 1);

        winnerTextObject.SetActive(true);
        winnerTextAnim.SetTrigger("GameOver");

        Transform winnerTransform = players[winnerIndex].transform;

        for (int i = 0; i < cameraTargets.Count; i++) {
            if (cameraTargets[i].target != winnerTransform) {
                RemoveCameraTarget(cameraTargets[i].target);
            }
        }

        StartCoroutine(BackToMenu());
    }

    IEnumerator BackToMenu() {
        yield return new WaitForSeconds(5f);

        uiController.GoTo("main-menu");
    }

    public void AddScore(int playerId, int scoreInc) {
        scores[playerId] += scoreInc;
        UpdateScoreText(playerId);
    }

    public void SubtractScore(int playerId, int scoreInc) {
        scores[playerId] -= scoreInc;
        UpdateScoreText(playerId);
    }

    void UpdateScoreText(int playerId) {
        texts[playerId].text = string.Format("P{0}: {1}", playerId + 1, scores[playerId]);
    }

    public int GetPlayerIdFromGameObject(GameObject toCheck) {
        for (int i = 0; i < players.Count; i++) {
            if (players[i] == toCheck) {
                return i;
            }
        }
        return -1;
    }
}
