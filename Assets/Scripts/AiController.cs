using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AiController : Agent, CheckpointAgent
{
    public Rigidbody carRb;
    public TrackCheckpoints trackCheckpoints;

    public Transform spawnPoint;
    //car detection
    public Transform[] otherCars;
    public List<Wheel> wheels;
    //timeout logic
    private int stepCount;
    public int maxEpisodeSteps = 300000;

    private float previousDistanceToCheckpoint;

    //vehicle stats
    public float maxAccelaration = 100.0f;
    public float brakeAcceleration = 50.0f;
    public float turnSensitivity = 1.5f;
    public float maxSteerAngle = 50.0f;
    public float topSpeed = 400.0f;
    public float brakeStrength = 300f;
    public Vector3 antiRoll;

    public enum Axel
    {
        Front,
        Rear
    }

    [System.Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void ResetStepTimer()
    {
        stepCount = 0;
    }
    public override void OnEpisodeBegin()
    {
        stepCount = 0;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        carRb.linearVelocity = Vector3.zero;
        carRb.angularVelocity = Vector3.zero;

        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = 0f;
            wheel.wheelCollider.brakeTorque = 0f;
            wheel.wheelCollider.steerAngle = 0f;

            
        }

        trackCheckpoints.ResetCheckpoints(transform);

        Transform nextCheckpoint = trackCheckpoints.GetNextCheckpoint(transform);

        if (nextCheckpoint != null)
        {
            previousDistanceToCheckpoint = Vector3.Distance(transform.position, nextCheckpoint.position);
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // speed
        sensor.AddObservation(transform.InverseTransformDirection(carRb.linearVelocity));

        // rotation stability
        sensor.AddObservation(carRb.angularVelocity.y);

        // direction to next checkpoint
        Transform nextCheckpoint = trackCheckpoints.GetNextCheckpoint(transform);

        if (nextCheckpoint == null)
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
        else
        {
            Vector3 dir = nextCheckpoint.position - transform.position;
            sensor.AddObservation(transform.InverseTransformDirection(dir.normalized));
            sensor.AddObservation(Vector3.Dot(transform.forward, dir.normalized));
        }
        float closestDist = 999f;
        Vector3 closestDir = Vector3.zero;

        foreach (var c in otherCars)
        {
            if (c == transform) continue;

            Vector3 diff = c.position - transform.position;
            float dist = diff.magnitude;

            if (dist < closestDist)
            {
                closestDist = dist;
                closestDir = diff.normalized;
            }
        }

        // normalized distance 
        sensor.AddObservation(closestDist / 20f);

        // direction to closest car
        sensor.AddObservation(transform.InverseTransformDirection(closestDir));
        sensor.AddObservation(RayDistance(0));    // front
        sensor.AddObservation(RayDistance(-30));  // left
        sensor.AddObservation(RayDistance(30));   // right
        //failsafe
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        carRb.centerOfMass = antiRoll;
        float steer = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float accel = Mathf.Clamp(actions.ContinuousActions[1], 0f, 1f);
        float brake = Mathf.Clamp(actions.ContinuousActions[2], 0f, 1f);

        stepCount++;

        if (stepCount > maxEpisodeSteps)
        {
            AddReward(-1f);
            stepCount = 0;
            EndEpisode();
            return;
        }

        foreach (var wheel in wheels)
        {
            // acceleration
            wheel.wheelCollider.motorTorque = accel * maxAccelaration;

            float speed = carRb.linearVelocity.magnitude;

            //brakes will not work if car is slow. Cheating, but needed
            if (speed > 45f)
            {
                wheel.wheelCollider.brakeTorque = brake * brakeStrength * 0.02f;
            } else {
                wheel.wheelCollider.brakeTorque = 0f;
            }
            // steering
            if (wheel.axel == Axel.Front)
            {
                float targetSteerAngle = steer * turnSensitivity * maxSteerAngle;

                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, targetSteerAngle, 0.4f);
            }
        }

        

        // small penalty to discourage idling
        AddReward(-0.001f);
        //prevent constant full lock steer
        AddReward(-Mathf.Abs(steer) * 0.002f);

        //temp out while braking doesnt exist
        float speed2 = carRb.linearVelocity.magnitude;
        //STOP BRAKING WHEN YOU'RE SLOW
        if (brake > 0.1f)
        {
            AddReward(-brake * 0.005f);
        }

        Transform nextCheckpoint = trackCheckpoints.GetNextCheckpoint(transform);

        if (nextCheckpoint != null)
        {
            Vector3 toCheckpoint = nextCheckpoint.position - transform.position;
            float distanceToCheckpoint = toCheckpoint.magnitude;
            float alignment = Vector3.Dot(transform.forward, toCheckpoint.normalized);
            float sidewaysSpeed = Mathf.Abs(Vector3.Dot(carRb.linearVelocity, transform.right));
            float progress = previousDistanceToCheckpoint - distanceToCheckpoint;float speed = carRb.linearVelocity.magnitude;
            float forwardSpeed = Vector3.Dot(carRb.linearVelocity, transform.forward);

            if (progress > 0.01f)
            {
                AddReward(progress * 0.2f);
            } else {
                AddReward(-0.001f);
            }

            //punish driving sideways
            AddReward(-sidewaysSpeed * 0.001f);

            //need minimum speed
            if (forwardSpeed < 14f)
            {
                AddReward(-0.001f);
            }

            previousDistanceToCheckpoint = distanceToCheckpoint;


            if (alignment > 0.5f && forwardSpeed > 0f)
            {
                AddReward((speed / 30f) * 0.005f);
            }
            //punish bad alignment
            if (alignment < 0f)
            {
                AddReward(-0.01f);
            }

            AddReward(alignment * 0.0001f);
        }
    }
    float RayDistance(float angle)
    {
        Quaternion rot = transform.rotation * Quaternion.Euler(0, angle, 0);
        Vector3 dir = rot * Vector3.forward;

        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, dir);

        if (Physics.Raycast(ray, out RaycastHit hit, 30f))
        {
            return hit.distance / 30f;
        }
        return 1f; // no wall detected
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Car"))
        {
            AddReward(-0.9f);
            //EndEpisode();
        }
        if (collision.transform.CompareTag("Wall"))
        {
            AddReward(-0.8f);
            //EndEpisode();


            Debug.Log("Crashed into wall");
        }
    }

    public void ResetCheckpointDistance()
    {
        Transform nextCheckpoint = trackCheckpoints.GetNextCheckpoint(transform);

        if (nextCheckpoint != null)
        {
            previousDistanceToCheckpoint = Vector3.Distance(transform.position, nextCheckpoint.position);
        }
    }
}