using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Stage : Object {
    private float m_stageTimer;
    private float m_stageLength;

    private bool m_stageComplete;

    public Stage(float stageLength) {
        m_stageLength = stageLength;

        m_stageTimer = 0;
        m_stageComplete = false;
    }

    // This is called by StageManager
    public void Update(float delatTime) {
        if (!m_stageComplete) {
            GameplayLoop(delatTime);
        }
    }

    // Implement in child class
    virtual protected void GameplayLoop(float deltaTime) {
        m_stageTimer += deltaTime;

        if (m_stageTimer > m_stageLength) {
            m_stageComplete = true;
            return;
        }
    }

    public bool IsStageComplete() {
        return m_stageComplete;
    }
}
