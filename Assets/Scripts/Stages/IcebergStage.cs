using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcebergStage : Stage {

    private enum IcebergProgression { InitialWait, Initialize, AnimEndWait, FinalInitEvent, End };
    private IcebergProgression m_currentState;

    private float m_timer;
    
    public float m_initialWaitTime = 1f;
    public float m_animEndWaitTime = 1.15f;

    public IcebergStage(float stageLength) : base(stageLength) {
        m_timer = 0;
    }

    protected override void GameplayLoop(float deltaTime) {
        base.GameplayLoop(deltaTime);

        switch (m_currentState) {
            case IcebergProgression.InitialWait:
                WaitUntilNextState(deltaTime, m_initialWaitTime, IcebergProgression.Initialize);
                break;

            case IcebergProgression.Initialize:
                StageManager.GetInstance().iceberg.SetActive(true);
                StageManager.GetInstance().boat.SetTrigger("Iceberg");

                m_currentState = IcebergProgression.AnimEndWait;
                break;

            case IcebergProgression.AnimEndWait:
                WaitUntilNextState(deltaTime, m_animEndWaitTime, IcebergProgression.FinalInitEvent);
                break;

            case IcebergProgression.FinalInitEvent:
                foreach (GameObject child in StageManager.GetInstance().platforms) {
                    child.SetActive(false);
                }

                StageManager.GetInstance().gm.AddCameraHookTarget(StageManager.GetInstance().goat);

                foreach (Rigidbody2D rb in StageManager.GetInstance().players) {
                    rb.AddForce(Vector2.up * 5000);
                }

                m_currentState = IcebergProgression.End;
                break;

            default:
                break;
        }
    }

    void WaitUntilNextState(float increment, float threshold, IcebergProgression nextState) {
        m_timer += increment;

        if (m_timer >= threshold) {
            m_currentState = nextState;
        }
    }
}
