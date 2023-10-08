using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

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

        //[SerializeField] private float minMaxPitch = 30f;
        //[SerializeField] private float minMaxRoll = 30f;
        //[SerializeField] private float maxThrottle = 5f;
        //[SerializeField] private float yawPower = 4f;

        [SerializeField] private float lerpSpeed = 2f;
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

        [SerializeField] private Vector3 DirToGoal;
        [SerializeField] private float DistToGoal;

        [Header("Ml Targets")] [SerializeField]
        private GameObject goal;

        [SerializeField] private GoalSpawner _goalSpawner;
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private Vector3 _myLocation;
        [SerializeField] private Vector3 _myVelo;

        [Header("RayTracing")] [SerializeField]
        private RayPerceptionSensorComponent3D raySensor;

        public float thresholdDistance = 1.5f;

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

            //transform.localPosition = new Vector3(-0.9f,4.15f,4.32f);
        }

        private void Update()
        {
            /* // Checking forward vector
                LineRenderer lineRenderer = GetComponent<LineRenderer>();
                 lineRenderer.SetVertexCount(2);
                 lineRenderer.SetPosition(0, transform.position);
                 lineRenderer.SetPosition(1, rb.transform.forward * 20 + transform.position);
                 */
        }

        #endregion

        #region ML

        public override void OnEpisodeBegin()
        {
            ResetValues();
            _goalSpawner.KillGoal();
            _goalSpawner.SpawnFood();
            // transform.localPosition = new Vector3(Random.Range(-3f, 3f), Random.Range(0.2f, 7f), Random.Range(-4f, 4f));
            transform.localPosition = new Vector3(0, 2, -8.5f);
            _targetPosition = _goalSpawner.GetLastGoalTransform();
        }


        public override void CollectObservations(VectorSensor sensor)
        {
            var rcComponents = GetComponents<RayPerceptionSensorComponent3D>();
            foreach (var rcComponent in rcComponents)
            {
                var rayInput = rcComponent.GetRayPerceptionInput();
                var rayResult = RayPerceptionSensor.Perceive(rayInput);

                foreach (var rayOutput in rayResult.RayOutputs)
                {
                    sensor.AddObservation(rayOutput.HasHit);
                    sensor.AddObservation(rayOutput.HitFraction);
                    sensor.AddObservation(rayOutput.HitTaggedObject);
                }
            }


            sensor.AddObservation(_goalSpawner.HasGoalSpawned());
            DistToGoal = Vector3.Distance(_goalSpawner.GetLastGoalTransform(), _myLocation);
            sensor.AddObservation(DistToGoal);
            DirToGoal = (_goalSpawner.GetLastGoalTransform() - _myLocation).normalized;
            sensor.AddObservation(DirToGoal);


            sensor.AddObservation(_normPitch);
            sensor.AddObservation(_normYaw);
            sensor.AddObservation(_normRoll);
            sensor.AddObservation(_normThrottle);
            sensor.AddObservation(_normFPitch);
            sensor.AddObservation(_normFYaw);
            sensor.AddObservation(_normFRoll);
            sensor.AddObservation(_normFThrottle);

            sensor.AddObservation(transform.localPosition.normalized);
            sensor.AddObservation(transform.localRotation);
            sensor.AddObservation(rb.velocity);
            sensor.AddObservation(rb.transform.forward.normalized);
        }


        public override void OnActionReceived(ActionBuffers actions)
        {
            //Debug.Log("forward" + rb.transform.forward);
            Debug.Log("RB angular" + rb.angularVelocity);

            _myLocation = transform.localPosition;

            _myVelo = rb.velocity;

            _pitch = actions.ContinuousActions[0] * maxPitch;
            _roll = actions.ContinuousActions[1] * maxRoll;
            _yaw += actions.ContinuousActions[2] * maxYaw;
            _throttle = actions.ContinuousActions[3] * maxThrottle;

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


            //Testing <-<-<-<  


            // Calculate the direction to the goal
            var targetDirection = (_goalSpawner.GetLastGoalTransform() - _myLocation).normalized;

            // Calculate the distance to the goal
            var distanceToGoal = Vector3.Distance(_goalSpawner.GetLastGoalTransform(), _myLocation.normalized);

            // Calculate the reward based on alignment with goal direction
            var alignmentReward = Vector3.Dot(rb.velocity, targetDirection) * (0.001f / MaxStep);

            // Calculate the reward penalty for moving away from the goal
            var oppositeDirectionReward = -Vector3.Dot(rb.velocity, -targetDirection) * (0.001f / MaxStep);

            // Calculate the reward based on proximity to the goal
            var distanceReward = Mathf.Clamp01(1f - distanceToGoal / thresholdDistance);
            distanceReward *= 0.001f / MaxStep; // Adjust the reward factor as needed

            // Calculate the reward based on drone's velocity
            var velocityReward = rb.velocity.magnitude * 0.001f; // Adjust the reward factor as needed

            // Combine alignment, opposite direction, distance, and velocity rewards
            var totalReward = alignmentReward + oppositeDirectionReward + distanceReward + velocityReward;

            // Check if the agent is currently facing the goal within a certain angle
            float angle = 15;
            if (Vector3.Angle(rb.transform.forward, targetDirection) < angle)
                // Add a reward for facing the goal
                totalReward += 0.001f / MaxStep;
            else
                // Add a penalty for not facing the goal
                totalReward -= 0.001f / MaxStep;

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
                        if (rayOutput.HitGameObject.CompareTag("Goal") && rayOutput.HitFraction < 1.0f)
                            //Debug.Log("Is close enough to Goal: " + rayOutput.HitFraction);
                            AddReward(0.1f / MaxStep);
                        else if (rayOutput.HitGameObject.CompareTag("Killer") && rayOutput.HitFraction < 0.06f)
                            //Debug.Log("DANGER! Close to Killer: " + rayOutput.HitFraction);
                            AddReward(-0.1f / MaxStep);
                        else if (rayOutput.HitGameObject.CompareTag("Ground") && rayOutput.HitFraction < 0.06f)
                            //Debug.Log("Ground is close, CAREFUL: " + rayOutput.HitFraction);
                            AddReward(-0.1f / MaxStep);
                    }
            }
            // Debug.Log("Current rewards" + GetCumulativeReward());
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
            groundMeshRenderer.material = loseMaterial;
            if (other.TryGetComponent(out Goal goal))
            {
                Debug.Log("Collided with" + other);
                SetReward(1f); //reward value is only relative to other rewards
                groundMeshRenderer.material = winMaterial;
                EndEpisode();
            }

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