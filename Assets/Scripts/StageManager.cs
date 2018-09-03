using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour {

    private static StageManager instance = null;

	public enum GameState {Boat, Storm, Iceberg};
	//public GameManager GM;
	public GameState state;
	public int gameDuration = 180;
    public int numStages = 3;

	public GameObject seagull;
	public Transform seagull_origin;

	public GameObject iceberg_small;
	public Transform iceberg_origin;

	public GameObject sky_clear;
	public GameObject sky_stormy;


	public Animator boat;
	public GameObject[] platforms;
	public List<Rigidbody2D> players;
	public GameObject iceberg;
    public Transform goat;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public AudioController ac;

    private int currentStage = 0;
    private List<Stage> stages;

	// Use this for initialization
	void Start () {

        if (instance == null) {
            instance = this;
        }
        else if (instance != null) {
            Destroy(gameObject);
        }

        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        ac = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>();
        
		players = new List<Rigidbody2D> ();

        stages = new List<Stage> {
            new BoatStage(gameDuration / numStages),
            new StormStage(gameDuration / numStages, 0.01f),
            new IcebergStage(gameDuration / numStages)
        };
    }
	
    public static StageManager GetInstance() {
        return instance;
    }

	// Update is called once per frame
	void Update () {
        if (currentStage < stages.Count) {
            stages[currentStage].Update(Time.deltaTime);
            if (stages[currentStage].IsStageComplete()) {
                currentStage++;
            }
        }
	}
    
	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "Player") {
			players.Add (coll.gameObject.GetComponent<Rigidbody2D> ());
		}
	}

	void OnTriggerExit2D(Collider2D coll) {
		if (coll.tag == "Player") {
			players.Remove (coll.gameObject.GetComponent<Rigidbody2D> ());
		}
	}
}
