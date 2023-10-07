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

        [Header("Ml Targets")] [SerializeField]
        private GameObject goal;

        [SerializeField] private GoalSpawner _goalSpawner;
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private Vector3 _myLocation;
        [SerializeField] private Vector3 _myRBlocation;
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
        }

        #endregion

        #region ML

        public override void OnEpisodeBegin()
        {
            ResetValues();
            _goalSpawner.KillGoal();
            _goalSpawner.SpawnFood();
            // transform.localPosition = new Vector3(Random.Range(-3f, 3f), Random.Range(0.2f, 7f), Random.Range(-4f, 4f));
            transform.localPosition = new Vector3(0, 4, -4);
            _targetPosition = _goalSpawner.GetLastGoalTransform();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            var rcComponents = GetComponents<RayPerceptionSensorComponent3D>();

            for (var i = 0; i < rcComponents.Length; i++)
            {
                var r1 = rcComponents[i];
                var r2 = r1.GetRayPerceptionInput();
                var r3 = RayPerceptionSensor.Perceive(r2);
                foreach (var rayOutput in r3.RayOutputs)
                {
                    //Debug.Log(rayOutput.HasHit+" "+rayOutput.HitTaggedObject+" "+rayOutput.HitTagIndex+" "+rayOutput.HitFraction);
                    sensor.AddObservation(rayOutput.HasHit);
                    sensor.AddObservation(rayOutput.HitFraction);
                }
            }

            if (_goalSpawner.HasGoalSpawned())
            {
                sensor.AddObservation(_targetPosition);
                var DistToGoal = Vector3.Distance(_goalSpawner.GetLastGoalTransform(), _myLocation);
                sensor.AddObservation(DistToGoal);
                var DirToGoal =
                    (_goalSpawner.GetLastGoalTransform() - _myLocation).normalized; //can change to dot later
                sensor.AddObservation(DirToGoal);
                Debug.Log("DistanceToGoal: " + DistToGoal);
                Debug.Log("DirectionToGoal: " + DirToGoal);
                Debug.Log("_targetPosition" + _goalSpawner.GetLastGoalTransform());
            }

            
            sensor.AddObservation(_normPitch);
            sensor.AddObservation(_normFPitch);
            sensor.AddObservation(_normYaw);
            sensor.AddObservation(_normFYaw);
            sensor.AddObservation(_normRoll);
            sensor.AddObservation(_normFRoll);
            sensor.AddObservation(_normThrottle);
            sensor.AddObservation(_normFThrottle);
            
            
            sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(transform.localRotation);
            sensor.AddObservation(transform.forward);

            sensor.AddObservation(rb.velocity);
            sensor.AddObservation(rb.transform.forward);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            _myLocation = transform.localPosition;
            _myVelo = rb.velocity;

            _pitch = actions.ContinuousActions[0] * maxPitch;
            _roll = actions.ContinuousActions[1] * maxRoll;
            _yaw += actions.ContinuousActions[2] * maxYaw;
            _throttle = actions.ContinuousActions[3] * maxThrottle;

            //_pitch = (actions.ContinuousActions[0] - minPitch) / (maxPitch - minPitch) * 2 - 1;
            //Normalization
            _normThrottle = (_throttle - minThrottle) / (maxThrottle - minThrottle) * 2 - 1;
            _normPitch = (_pitch - minPitch) / (maxPitch - minPitch) * 2 - 1;
            _normRoll = (_roll - minRoll) / (maxRoll - minRoll) * 2 - 1;
            _normYaw = (_yaw - minYaw) / (maxYaw - minYaw) * 2 - 1;

            _finalThrottle = Mathf.Lerp(_finalThrottle, _throttle, Time.deltaTime * lerpSpeed);
            _finalPitch = Mathf.Lerp(_finalPitch,_pitch , Time.deltaTime * lerpSpeed);
            _finalRoll = Mathf.Lerp(_finalRoll, _roll, Time.deltaTime * lerpSpeed);
            _finalYaw = Mathf.Lerp(_finalYaw, _yaw, Time.deltaTime * lerpSpeed);

            
            _normFPitch = (_finalPitch - minPitch) / (maxPitch - minPitch) * 2 - 1;
            _normFRoll = (_finalRoll - minRoll) / (maxRoll - minRoll) * 2 - 1;
            _normFYaw = (_finalYaw - minYaw) / (maxYaw - minYaw) * 2 - 1;
            _normFThrottle = (_finalThrottle - minThrottle) / (maxThrottle - minThrottle) * 2 - 1;
            

            //var rot = Quaternion.Euler(_normFPitch, _normFYaw, -_normFRoll);
            var rot = Quaternion.Euler(_finalPitch, _finalYaw, -_finalRoll);
            //var normRot = rot.eulerAngles / 180.0f - Vector3.one; // [-1,1]
            //Quaternion normQRot = Quaternion.Euler(normRot.x, normRot.z, normRot.y);
            //rb.MoveRotation(normQRot);
            rb.MoveRotation(rot);
           
            
            //rb.AddRelativeForce(0,normFThrottle,0);

            //rb.AddRelativeForce(new Vector3(0, _finalThrottle, 0));
            /*
            float angle = 20;
            if (Vector3.Angle(rb.transform.forward, _goalSpawner.GetLastGoalTransform() - rb.position) <
                angle)
            {
                //Debug.Log("Is currently facing goal");
                AddReward(0.1f / MaxStep);
            }
            else
            {
                //Debug.Log("Is Not Facing the goal !!");
                AddReward(-0.1f / MaxStep);
            }
            */

            //Testing <-<-<-<
            /*
            Vector3 targetDirection = (_goalSpawner.GetLastGoalTransform() - _myLocation).normalized;
            AddReward(Vector3.Dot(rb.velocity, targetDirection) * (0.1f / MaxStep));
            */
            var rcComponents = GetComponents<RayPerceptionSensorComponent3D>();

            for (var i = 0; i < rcComponents.Length; i++)
            {
                var r1 = rcComponents[i];
                var r2 = r1.GetRayPerceptionInput();
                var r3 = RayPerceptionSensor.Perceive(r2);
                foreach (var rayOutput in r3.RayOutputs)
                {
                    if (rayOutput.HasHit && rayOutput.HitGameObject.CompareTag("Goal"))
                        if (rayOutput.HitFraction < 0.1f)
                        {
                            Debug.Log("Is close enough to Goal" + rayOutput.HitFraction);
                            AddReward(0.1f / MaxStep);
                        }

                    if (rayOutput.HasHit && rayOutput.HitGameObject.CompareTag("Killer"))
                        if (rayOutput.HitFraction < 0.1f)
                        {
                            Debug.Log("DANGER! Close to Killer" + rayOutput.HitFraction);
                            AddReward(-0.1f / MaxStep);
                        }

                    if (rayOutput.HasHit && rayOutput.HitGameObject.CompareTag("Ground"))
                        if (rayOutput.HitFraction < 0.1f)
                        {
                            Debug.Log("Ground is close , CAREFULL: " + rayOutput.HitFraction);
                            AddReward(-0.1f / MaxStep);
                        }
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

        private void OnTriggerEnter(Collider other)
        {
            groundMeshRenderer.material = loseMaterial;
            if (other.TryGetComponent(out Goal goal))
            {
                Debug.Log("Collided with" + other);
                SetReward(+1f); //reward value is only relative to other rewards
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
                //SetReward(-1f);
                //EndEpisode();
                //groundMeshRenderer.material = loseMaterial;
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
            rb.angularVelocity = Vector3.zero;

            _pitch = 0;
            _roll = 0;
            _yaw = 0;
            _throttle = 0;

            _finalPitch = 0;
            _finalRoll = 0;
            _finalYaw = 0;
            _finalThrottle = 0;
        }

        #endregion
    }
}