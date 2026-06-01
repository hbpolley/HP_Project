using System;
using UnityEngine;
using System.Collections.Generic;

public class TrackCheckpoints : MonoBehaviour
{

    [SerializeField] private List<Transform> carTransformList;
    public event EventHandler OnPlayerCorrectCheckpoint;
    public event EventHandler OnPlayerWrongCheckpoint;
    private List<Checkpoint> checkpointSingleList;
    private List<int> nextCheckpointIndexList;
    private int LapCount;
    public int lapsToFinish = 3;
    public event EventHandler OnRaceFinished;
    private bool raceFinished;
    public event EventHandler OnLapCompleted;

    private Dictionary<Transform, int> lapCountDictionary;

    private void Awake()
    {
        Transform checkpointsTransform = transform.Find("Checkpoints");
        LapCount = 1;

        checkpointSingleList = new List<Checkpoint>();
        lapCountDictionary = new Dictionary<Transform, int>();

        foreach (Transform checkpointSingleTransform in checkpointsTransform)
        {
            Checkpoint checkpointSingle = checkpointSingleTransform.GetComponent<Checkpoint>();
            checkpointSingle.SetTrackCheckpoints(this);
            checkpointSingleList.Add(checkpointSingle);
            

        }

        nextCheckpointIndexList = new List<int>();
        foreach (Transform carTransform in carTransformList)
        {
            nextCheckpointIndexList.Add(0);
            lapCountDictionary[carTransform] = 1;
        }
    }

    public void CarThroughCheckpoint(Checkpoint checkpointSingle, Transform carTransform)
    {
        CheckpointAgent agent = carTransform.GetComponentInParent<CheckpointAgent>();

        if (agent == null)
            return;

        int carIndex = carTransformList.IndexOf(carTransform);

        if (carIndex == -1)
            return;

        int expectedIndex = nextCheckpointIndexList[carIndex];
        int actualIndex = checkpointSingleList.IndexOf(checkpointSingle);

        if (actualIndex == expectedIndex)
        {
            agent.AddReward(1f);
            //reset the timer
            AiController ai = carTransform.GetComponent<AiController>();

            if (ai != null)
            {
                ai.ResetStepTimer();
                ai.ResetCheckpointDistance();
            }

            int nextIndex = (expectedIndex + 1) % checkpointSingleList.Count;
            nextCheckpointIndexList[carIndex] = nextIndex;

            if (expectedIndex == checkpointSingleList.Count - 1)
            {
                agent.AddReward(5f);
                lapCountDictionary[carTransform]++;
                OnLapCompleted?.Invoke(this, EventArgs.Empty);
                // Check if the player has finished the race
                if (lapCountDictionary[carTransform] > lapsToFinish && !raceFinished) //starts on lap 1, so when the game registers lap 4 we are done
                {
                    //failsafe
                    raceFinished = true;

                    // Notify listeners (UI, race manager, etc.)
                    OnRaceFinished?.Invoke(this, EventArgs.Empty);

                    Debug.Log("Race Finished!");
                }
            }
        }
        else
        {
            agent.AddReward(-0.5f);
        }
    }
    
    public Transform GetNextCheckpoint(Transform carTransform)
    {
        int carIndex = carTransformList.IndexOf(carTransform);

        if (carIndex == -1)
        {
            return null;
        }
        int nextCheckpointIndex = nextCheckpointIndexList[carIndex];

        if (checkpointSingleList == null || checkpointSingleList.Count == 0)
        {
            return null;
        }

        return checkpointSingleList[nextCheckpointIndex].transform;
    }

    public void ResetCheckpoints(Transform carTransform)
    {
        int carIndex = carTransformList.IndexOf(carTransform);

        if (carIndex == -1)
        {
            return;
        }

    nextCheckpointIndexList[carIndex] = 0;
    }
    public int GetLapCount(Transform carTransform)
    {
        if (lapCountDictionary.ContainsKey(carTransform))
        {
            return lapCountDictionary[carTransform];
        }
        //in case the car doesnt exist
        return 1;
    }
}

