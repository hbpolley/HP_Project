using UnityEngine;

public interface CheckpointAgent
{
    Transform GetTransform();
    void AddReward(float reward);
    void EndEpisode();
}