using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private TrackCheckpoints trackCheckpoints;

    private void OnTriggerEnter(Collider other)
    {
        CheckpointAgent agent = other.GetComponentInParent<CheckpointAgent>();

        if (agent != null)
        {
            trackCheckpoints.CarThroughCheckpoint(this, agent.GetTransform());
        }
    }

    public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints)
    {
        this.trackCheckpoints = trackCheckpoints;
    }
}
