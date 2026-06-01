using UnityEngine;

public class PlayerCheckpointAdapter : MonoBehaviour, CheckpointAgent
{
    public Transform GetTransform()
    {
        return transform;
    }

    public void AddReward(float reward) { }

    public void EndEpisode() { }

    public void SetCheckpointIndex(int index) { }
}