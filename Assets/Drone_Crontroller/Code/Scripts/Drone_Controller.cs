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
        private float minThrottle = -5f;

        [SerializeField] private float maxThrottle = 3f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 30f;
        [SerializeField] private float minRoll = -30f;
        [SerializeField] private float maxRoll = 30f;
        [SerializeField] private float minYaw = -3f;
        [SerializeField] private float maxYaw = 3f;
        [SerializeField] private float lerpSpeed = 2f;

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

        [SerializeField] private Transform spawnPosition;
        public float rotationSpeed = 5f;
            public float radius = 2f;


        [Header("Ml Targets")]
        // private GameObject goal;
        [SerializeField]
        private Vector3 DirToGoal;

        [SerializeField] private float DistToGoal;
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private Vector3 _myLocation;
        [SerializeField] private Vector3 _myVelo;
        [SerializeField] private Quaternion rbStartRotation;

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

            _trackCheckpoints.OnDroneCorrectCheckpoint += TrackCheckpoints_OnDroneCorrectCheckpoint;
            _trackCheckpoints.OnDroneWrongCheckpoint += TrackCheckpoints_OnDroneWrongCheckpoint;


            //transform.localPosition = new Vector3(-0.9f,4.15f,4.32f);

        }

        #endregion

        #region ML

        public override void OnEpisodeBegin()
        {
            
            transform.position = spawnPosition.position =
               new Vector3(Random.Range(-64f, -50f), Random.Range(5f, 14f), Random.Range(-25f, -40f));
            transform.forward = spawnPosition.forward;
            _trackCheckpoints.ResetCheckPoint(transform);

            ResetValues();
        }

        private void Update()
        {
            VisualizeForward();
            
            
        // Calculate the new position on the circle
        float angle = Time.time * rotationSpeed;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        // Update the object's position
        transform.position = new Vector3(x, transform.position.y, z);

        // Rotate the object around the Y-axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            _targetPosition = _trackCheckpoints.GetNextCheckpointPosition(transform).transform.localPosition;
            //works
            Vector3 checkpointForward = _trackCheckpoints.GetNextCheckpointPosition(transform).transform.forward;
            float directionDot = Vector3.Dot(constantForward, checkpointForward);
            //Debug.Log("Direction to checkpoint" + directionDot);
            sensor.AddObservation(directionDot);

            //works 
            DistToGoal = Vector3.Distance(_trackCheckpoints.GetNextCheckpointlocation(transform), _myLocation);
            //Debug.Log("dist to goal" + DistToGoal);
            sensor.AddObservation(DistToGoal);
            DirToGoal = (_trackCheckpoints.GetNextCheckpointlocation(transform) - _myLocation).normalized;
            // Debug.Log("dir to goal" + DirToGoal);
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

            sensor.AddObservation(transform.localPosition.normalized);
            sensor.AddObservation(transform.localRotation);
            sensor.AddObservation(rb.velocity);
            sensor.AddObservation(rb.transform.forward.normalized);
            
        }


        public override void OnActionReceived(ActionBuffers actions)
        {
            //Debug.Log("forward" + rb.transform.forward);

            _myLocation = transform.localPosition;

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
            var alignmentReward = velocityDotGoal * (0.10f / MaxStep);

// Calculate the reward based on proximity to the goal
            var distanceReward = Mathf.Clamp01(1f - DistToGoal / thresholdDistance);
            distanceReward *= 0.1f / MaxStep; // Adjust the reward factor as needed


// Combine alignment, distance, velocity, and direction rewards
            var totalReward = alignmentReward + distanceReward;
            
            // Calculate the dot product between the agent's forward direction and the direction to the checkpoint
            float dotProduct = Vector3.Dot(constantForward, DirToGoal);
            if (dotProduct > 0.94f)
            {
                totalReward += (1.0f / MaxStep);
            }
            else
            {
                totalReward -= (3.0f / MaxStep);
            }


            AddReward(totalReward);


            //Debug.Log("totalREward" + totalReward); 


            var rcComponents = GetComponents<RayPerceptionSensorComponent3D>();

            foreach (var rcComponent in rcComponents)
            {
                var rayInput = rcComponent.GetRayPerceptionInput();
                var rayResult = RayPerceptionSensor.Perceive(rayInput);
                
                foreach (var rayOutput in rayResult.RayOutputs)
                    if (rayOutput.HasHit)
                    {
                        
                        if (rayOutput.HitGameObject.CompareTag("Checkpoints") && rayOutput.HitFraction < 0.1f)
                        {
                            // Reward based on the distance fraction to the goal
                            var checkpointReward = 0.2f * rayOutput.HitFraction / MaxStep;
                            AddReward(checkpointReward);
                        }
                        
                        else if (rayOutput.HitGameObject.CompareTag("Killer") && rayOutput.HitFraction < 0.04f)
                        {
                            // Penalty based on the distance fraction to the killer object
                            var killerPenalty = -0.5f * rayOutput.HitFraction / MaxStep;
                            AddReward(killerPenalty);
                        }
                        else if (rayOutput.HitGameObject.CompareTag("Ground") && rayOutput.HitFraction < 0.04f)
                        {
                            // Penalty based on the distance fraction to the ground
                            var groundPenalty = -0.5f * rayOutput.HitFraction / MaxStep;
                            AddReward(groundPenalty);
                        }
                    }
            }

            // Debug.Log("Current rewards" + GetCumulativeReward());
        }

        private void TrackCheckpoints_OnDroneWrongCheckpoint(object sender, TrackCheckpoints.DroneCheckPointEventArgs e)
        {
            if (e.droneTransform == transform)
            {
                AddReward(-1f);
                groundMeshRenderer.material = loseMaterial;
            }
        }

        private void TrackCheckpoints_OnDroneCorrectCheckpoint(object sender,
            TrackCheckpoints.DroneCheckPointEventArgs e)
        {
            if (e.droneTransform.transform == transform)
            {
                Debug.Log("Adding a rewards from track check point");
                AddReward(1.0f);
                groundMeshRenderer.material = winMaterial;
            }
        }


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
            /*
            if (other.TryGetComponent(out Goal goal))
            {
                Debug.Log("Collided with" + other);
                SetReward(1f); //reward value is only relative to other rewards
                groundMeshRenderer.material = winMaterial;
                EndEpisode();
            }
            */
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
            //HandleControls();
        }

        protected virtual void HandleEngines()
        {
            //rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude));
            foreach (var _engine in _engines) _engine.UpdateEngine(rb, _input);
        }

        /*
        protected virtual void HandleControls()
        {

        }
        */

        void VisualizeForward()
        {
            Debug.DrawRay(transform.position, constantForward * vectorLength, Color.blue);
        }
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