using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TdsWork
{
    [RequireComponent(typeof(Drone_Inputs))]
    public class Drone_Controller : Base_RigidBody
    {
        #region Variables

        [Header("Control Properties")] [SerializeField]
        private float minThrottle = -30f;

        [SerializeField] private float maxThrottle = 30f;
        [SerializeField] private float minPitch = -55f;
        [SerializeField] private float maxPitch = 55f;
        [SerializeField] private float minRoll = -55f;
        [SerializeField] private float maxRoll = 55f;
        [SerializeField] private float minYaw = -5f;
        [SerializeField] private float maxYaw = 5f;
        [SerializeField] private float lerpSpeed = 6f;

        public float thresholdDistance = 5;
        public float vectorLength = 5.0f;
        private Vector3 constantForward = Vector3.forward;

        private float _pitch;
        private float _finalPitch;
        private float _normPitch;
        private float _normFPitch;
        private float _roll;
        private float _finalRoll;
        private float _normRoll;
        private float _normFRoll;
        private float _yaw;
        private float _finalYaw;
        private float _normYaw;
        private float _normFYaw;
        private float _throttle;
        private float _finalThrottle;
        private float _normThrottle;
        private float _normFThrottle;

        [Header("Track/Checkpoints")] [SerializeField]
        private TrackCheckpoints _trackCheckpoints;

        private CheckpointSingle _checkpointSingle;

        [SerializeField] private Transform spawnPosition;


        [Header("Ml Targets")]
        [SerializeField]
        private Vector3 DirToGoal;

        [SerializeField] private float DistToGoal;
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private Vector3 _myLocation;
        [SerializeField] private Vector3 _myVelo;
        [SerializeField] private Quaternion rbStartRotation;
        [SerializeField] private Vector3 checkpointForward;
        [SerializeField] private float directionDot;
        private Vector3 StartPosition;

        [Header("RayTracing")] [SerializeField]
        private RayPerceptionSensorComponent3D raySensor;


        [Header("Materials")] [SerializeField] private Material winMaterial;

        [SerializeField] private Material loseMaterial;
        [SerializeField] private Material startMaterial;
        [SerializeField] private MeshRenderer groundMeshRenderer;


        private Drone_Inputs _input;
        private List<IEngine> _engines = new();

        #endregion

        #region Main Methods

        private void Start()
        {
            _input = GetComponent<Drone_Inputs>(); // grab the instance of the drone inputs;
            _engines = GetComponentsInChildren<IEngine>().ToList();

            raySensor = GetComponent<RayPerceptionSensorComponent3D>();
            
            
            // subscribe to the events
            _trackCheckpoints.OnDroneCorrectCheckpoint += TrackCheckpoints_OnDroneCorrectCheckpoint;
            _trackCheckpoints.OnDroneLastCheckpoint += TrackCheckpoints_onDroneLastCheckpoint;
            _trackCheckpoints.OnDroneWrongCheckpoint += TrackCheckpoints_OnDroneWrongCheckpoint;

            StartPosition = transform.localPosition;
        }

        #endregion

        #region ML

        public override void OnEpisodeBegin()
        {
            ResetValues(); // reset the values
            // randomize the position of the drone
            transform.localPosition = StartPosition + new Vector3(Random.Range(-20f, 20f), Random.Range(-15f, 15f), Random.Range(-15f, 15f));
            spawnPosition.localPosition = transform.localPosition;
            //spawnPosition.position = transform.position;
            transform.forward = spawnPosition.forward;
            // resets the checkpoints so that the drone can go through them again
            _trackCheckpoints.ResetCheckPoint(transform);
        }

        private void Update()
        {
            VisualizeForward(); // visualize the forward vector
            checkpointForward = _trackCheckpoints.GetNextCheckpointPosition(transform).transform.forward; // get the forward vector of the next checkpoint
            _targetPosition = _trackCheckpoints.GetNextCheckpointPosition(transform).transform.localPosition; // get the position of the next checkpoint
            DistToGoal = Vector3.Distance(_trackCheckpoints.GetNextCheckpointPosition(transform).transform.localPosition, transform.localPosition); // get the distance to the next checkpoint
            DirToGoal = (_trackCheckpoints.GetNextCheckpointlocation(transform) - transform.localPosition).normalized; // get the direction to the next checkpoint
            directionDot = Vector3.Dot(constantForward, checkpointForward); // get the dot product between the forward vector of the drone and the forward vector of the next checkpoint
        }

        public override void CollectObservations(VectorSensor sensor)
        {
         // adding the calculations from the update method to the observations 
            sensor.AddObservation(directionDot);
            sensor.AddObservation(DistToGoal);
            sensor.AddObservation(DirToGoal);
            sensor.AddObservation(_targetPosition);
            sensor.AddObservation(Vector3.Dot(rb.velocity, DirToGoal));
            sensor.AddObservation(Vector3.Dot(constantForward, DirToGoal));

            sensor.AddObservation(_normPitch);
            sensor.AddObservation(_normFPitch);

            sensor.AddObservation(_normYaw);
            sensor.AddObservation(_normFYaw);

            sensor.AddObservation(_normRoll);
            sensor.AddObservation(_normFRoll);

            sensor.AddObservation(_normThrottle);
            sensor.AddObservation(_normFThrottle);

            // adding some of the rigidbody values to the observations
            sensor.AddObservation(transform.localPosition.normalized);
            sensor.AddObservation(transform.localRotation);
            sensor.AddObservation(rb.velocity);
            sensor.AddObservation(rb.transform.forward.normalized);
            sensor.AddObservation(rb.rotation.normalized);
        }


        public override void OnActionReceived(ActionBuffers actions)
        {
            // input values from the drone inputs
            _myVelo = rb.velocity;
            _pitch = actions.ContinuousActions[0] * maxPitch;
            _roll = actions.ContinuousActions[1] * maxRoll;
            _yaw += actions.ContinuousActions[2] * maxYaw;
            _throttle = actions.ContinuousActions[3] * maxThrottle;


            // Calculate rotation without affecting yaw
            var rotationWithoutYaw = Quaternion.Euler(0, _finalYaw, -_finalRoll);

            // Calculate constant forward vector in world space
            constantForward = rotationWithoutYaw * Vector3.forward;

            // Normalize input values
            _normThrottle = Mathf.InverseLerp(minThrottle, maxThrottle, _throttle) * 2 - 1;
            _normPitch = Mathf.InverseLerp(minPitch, maxPitch, _pitch) * 2 - 1;
            _normRoll = Mathf.InverseLerp(minRoll, maxRoll, _roll) * 2 - 1;
            _normYaw = Mathf.InverseLerp(minYaw, maxYaw, _yaw) * 2 - 1;

            // Calculate final values with lerping
            _finalThrottle = Mathf.Lerp(_finalThrottle, _throttle, Time.deltaTime * lerpSpeed);
            _finalPitch = Mathf.Lerp(_finalPitch, _pitch, Time.deltaTime * lerpSpeed);
            _finalRoll = Mathf.Lerp(_finalRoll, _roll, Time.deltaTime * lerpSpeed);
            _finalYaw = Mathf.Lerp(_finalYaw, _yaw, Time.deltaTime * lerpSpeed);

            // Normalize final values
            _normFPitch = Mathf.InverseLerp(minPitch, maxPitch, _finalPitch) * 2 - 1;
            _normFRoll = Mathf.InverseLerp(minRoll, maxRoll, _finalRoll) * 2 - 1;
            _normFYaw = Mathf.InverseLerp(minYaw, maxYaw, _finalYaw) * 2 - 1;
            _normFThrottle = Mathf.InverseLerp(minThrottle, maxThrottle, _finalThrottle) * 2 - 1;

            // Calculate rotation
            var rot = Quaternion.Euler(_finalPitch, _finalYaw, -_finalRoll);

            // Apply rotation and force
            rb.MoveRotation(rot);
            rb.AddRelativeForce(0, _finalThrottle, 0);


            // Calculate the dot product between velocity and goal direction
            var velocityDotGoal = Vector3.Dot(rb.velocity, DirToGoal);

            // Calculate the reward based on alignment with goal direction
            var alignmentReward = velocityDotGoal * (0.15f / MaxStep);

            // Calculate the reward based on proximity to the goal
            var distanceReward = Mathf.Clamp01(1f - DistToGoal / thresholdDistance);
            distanceReward *= 0.1f / MaxStep; // Adjust the reward factor as needed

            // Combine alignment, distance, velocity, and direction rewards
            var totalReward = alignmentReward + distanceReward;

            // Calculate the dot product between the agent's forward direction and the direction to the checkpoint
            float dotProduct = Vector3.Dot(constantForward, DirToGoal);
            if (dotProduct > 0.91f && velocityDotGoal > 1.5f)
            {
                totalReward += (1.0f / MaxStep);
            }
            else
            {
                totalReward -= (1.0f / MaxStep);
            }

            AddReward(totalReward);

            var rcComponents = GetComponentsInChildren<RayPerceptionSensorComponent3D>();
        }

        // this is called when the drone goes through the wrong checkpoint
        private void TrackCheckpoints_OnDroneWrongCheckpoint(object sender, TrackCheckpoints.DroneCheckPointEventArgs e)
        {
            if (e.droneTransform == transform)
            {
                AddReward(-1f);
                groundMeshRenderer.material = loseMaterial;
            }
        }
       // this is called when the drone goes through the last checkpoint 
        private void TrackCheckpoints_onDroneLastCheckpoint(object sender,
            TrackCheckpoints.DroneCheckPointEventArgs e)
        {
            if (e.droneTransform == transform)
            {
                AddReward(5.0f * (10000.0f / MaxStep));
                EndEpisode();
                groundMeshRenderer.material = winMaterial;
            }
        }
       // this is called when the drone goes through the correct checkpoint
        private void TrackCheckpoints_OnDroneCorrectCheckpoint(object sender,
            TrackCheckpoints.DroneCheckPointEventArgs e)
        {
            if (e.droneTransform.transform == transform)
            {
                //  Debug.Log("Adding a rewards from track check point");
                AddReward(1.0f);
                groundMeshRenderer.material = winMaterial;
            }
        }

        // input values from the drone inputs 
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            //Debug.Log("Entering Heuristics:");
            var continousActions = actionsOut.ContinuousActions;
            continousActions[0] = _input.Cyclic.y;
            continousActions[1] = _input.Cyclic.x;
            continousActions[2] = _input.Pedals;
            continousActions[3] = _input.Throttle;
        }

        private void InferenceLogic()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            // if the drone collides with the ground or the killer object, the episode ends . splitted into two functions if i want different penalties
            if (other.TryGetComponent(out Ground ground))
            {
                SetReward(-1f);
                EndEpisode();
                groundMeshRenderer.material = loseMaterial;
            }

            if (other.TryGetComponent(out Killer killer))
            {
                // Debug.Log("Collided with " + other);
                SetReward(-1f);
                EndEpisode();
                groundMeshRenderer.material = loseMaterial;
            }
        }

        #endregion

        #region Custom Methods

        protected override void HandlePhysics() //This will not run if there is no rigidbody in rb script,
        {
            HandleEngines();
        }

        protected virtual void HandleEngines()
        {
            //rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude));
            foreach (var _engine in _engines) _engine.UpdateEngine(rb, _input);
        }

        void VisualizeForward()
        {
            Debug.DrawRay(transform.position, constantForward * vectorLength, Color.blue);
        }

        // reset the values
        private void ResetValues()
        {
            var startRot = quaternion.identity;
            rb.rotation = startRot;
            transform.localRotation = startRot;
            rb.velocity = Vector3.zero;

            _pitch = 0;
            _roll = 0;
            _yaw = 0;
            _throttle = 0;

            _finalPitch = 0;
            _finalRoll = 0;
            _finalYaw = 0;
            _finalThrottle = 0f;

            _normPitch = 0;
            _normRoll = 0;
            _normYaw = 0;
            _normThrottle = 0;

            _normFPitch = 0;
            _normFRoll = 0;
            _normFYaw = 0;
            _normFThrottle = 0;
        }

        #endregion
    }
}