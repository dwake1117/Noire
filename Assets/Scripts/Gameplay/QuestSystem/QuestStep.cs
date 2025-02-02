using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class QuestStep : MonoBehaviour
{
    private bool isFinished = false;
    private string questId;
    private int stepIndex;

    public void InitializeQuestStep(string questId, int stepIndex, string questStepState)
    {
        this.questId = questId;
        this.stepIndex = stepIndex;
        if (!String.IsNullOrEmpty(questStepState))
        {
            SetQuestStepState(questStepState);
        }
    }

    protected void FinishQuestStep()
    {
        if (!isFinished)
        {
            isFinished = true;
            GameEventsManager.Instance.QuestEvents.AdvanceQuest(questId);
            Destroy(gameObject);
        }
    }

    protected void ChangeState(string newState)
    {
        GameEventsManager.Instance.QuestEvents.QuestStepStateChange(questId, stepIndex, new QuestStepState(newState));
    }

    protected abstract void SetQuestStepState(string state);
}