using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


namespace TdsWork
{
    [RequireComponent(typeof(Drone_Inputs))]
    public class Drone_Controller : Base_RigidBody
    {
        #region Variables

        [Header("Control Properties")] 
        [SerializeField]private float minMaxPitch = 30f;
        [SerializeField]private float minMaxRoll = 35f;
        [SerializeField]private float yawPower = 4f;
        [SerializeField] private float maxThrottle = 5f;
        [SerializeField] private float lerpSpeed = 2f;

        [Header("Ml Targets")] [SerializeField]
        private GameObject goal;
        private Vector3 _targetTransform;
        private int maxsteps;

        [Header("RayTracing")] [SerializeField]
        private RayPerceptionSensorComponent3D raySensor;
        public float thresholdDistance = 1.5f;
        
        private Drone_Inputs _input;
        private List<IEngine> _engines = new List<IEngine>();

        private float _pitch;
        private float _finalPitch;
        private float _roll;
        private float _finalRoll;
        private float _yaw;
        private float _finalYaw;
        private float _throttle;
        private float _finalThrottle;
        #endregion

        #region Main Methods

        private void Start()
        {
            _input = GetComponent<Drone_Inputs>(); // grab the instance of the drone inputs;
            _engines = GetComponentsInChildren<IEngine>().ToList<IEngine>();

            raySensor = GetComponent<RayPerceptionSensorComponent3D>();
            
               // transform.localPosition = new Vector3(Random.Range(-3.5f, +3.5f), Random.Range(-1f,5f), Random.Range(+2.5f, +5f));
               transform.localPosition = new Vector3(-0.9f,4.15f,14.32f);
                goal = Instantiate(goal, this.transform.position, Quaternion.identity);
            //Physics.IgnoreCollision(this.GetComponent<Collider>(), GameObject.FindGameObjectWithTag("dr").GetComponent<Collider>());
        }

        private void Update()
        {
        }

        #endregion

        #region ML

        public override void OnEpisodeBegin()
        {
            transform.localPosition = new Vector3(0,4f,0);
            rb.velocity = Vector3.zero;
            _targetTransform = goal.gameObject.transform.position;
            _yaw = 0;
            _finalYaw = 0;
        }

        public override void CollectObservations(VectorSensor sensor)
        {

            var rcComponents = this.GetComponents<RayPerceptionSensorComponent3D>();
            
                        for (int i = 0; i < rcComponents.Length; i++)
                        {
                            var r1 = rcComponents[i];
                            var r2 = r1.GetRayPerceptionInput();
                            var r3 = RayPerceptionSensor.Perceive(r2);
                            foreach (RayPerceptionOutput.RayOutput rayOutput in r3.RayOutputs)
                            {
                                //Debug.Log(rayOutput.HasHit+" "+rayOutput.HitTaggedObject+" "+rayOutput.HitTagIndex+" "+rayOutput.HitFraction);
                               sensor.AddObservation(rayOutput.HitFraction);
                               sensor.AddObservation(rayOutput.HasHit);
                            }
                            
                        }
                        
            sensor.AddObservation(_targetTransform);
            Vector3 DirToGoal = (_targetTransform - transform.localPosition).normalized; //can change to dot later
            Debug.Log("Direction: " + DirToGoal);
            Debug.Log("_targetTransform" + _targetTransform);
            sensor.AddObservation(DirToGoal.x);
            sensor.AddObservation(DirToGoal.y);
            sensor.AddObservation(DirToGoal.z);
            
            sensor.AddObservation(_pitch);
            sensor.AddObservation(_finalPitch);
            sensor.AddObservation(_yaw);
            sensor.AddObservation(_finalYaw);
            sensor.AddObservation(_roll);
            sensor.AddObservation(_finalRoll);
            sensor.AddObservation(_throttle);
            sensor.AddObservation(_finalThrottle);
            sensor.AddObservation(rb.velocity);
            sensor.AddObservation(rb.position);
            sensor.AddObservation(rb.transform.forward);
            sensor.AddObservation(rb.position.y);
            
            
            
            

            
        }

        public override void OnActionReceived(ActionBuffers actions)
        {


            // var r1 = this.GetComponent<RayPerceptionSensorComponent3D>();
            // var r2 = r1.GetRayPerceptionInput();  
            //var r3 =  RayPerceptionSensor.Perceive(r2);

            _pitch = actions.ContinuousActions[0] * minMaxPitch;
            _roll = actions.ContinuousActions[1] * minMaxRoll;
            _yaw += actions.ContinuousActions[2] * yawPower;
            _throttle = actions.ContinuousActions[3] * maxThrottle;

            _finalPitch = Mathf.Lerp(_finalPitch, _pitch, Time.deltaTime * lerpSpeed);
            _finalRoll = Mathf.Lerp(_finalRoll, _roll, Time.deltaTime * lerpSpeed);
            _finalYaw = Mathf.Lerp(_finalYaw, _yaw, Time.deltaTime * lerpSpeed);
            _finalThrottle = Mathf.Lerp(_finalThrottle, _throttle, Time.deltaTime * lerpSpeed);

            Quaternion rot = Quaternion.Euler(_finalPitch, _finalYaw, _finalRoll);
            //Add torque later
            rb.MoveRotation(rot);
            rb.AddRelativeForce(new Vector3(0, _finalThrottle, 0));

            
            if (rb.position.y > 0.35f)
            {
                AddReward(0.1f / MaxStep); //Only add after they have learned first
            }
            
            
            if (_finalPitch > 0.5f || _finalPitch < 0.5f)
            {
                AddReward(0.1f / MaxStep);
            }
            if (_finalRoll > 0.5f || _finalRoll < 0.5f)
            {
                AddReward(0.1f / MaxStep);
            }
            
            var rcComponents = this.GetComponents<RayPerceptionSensorComponent3D>();

            for (int i = 0; i < rcComponents.Length; i++)
            {
                var r1 = rcComponents[i];
                var r2 = r1.GetRayPerceptionInput();
                var r3 = RayPerceptionSensor.Perceive(r2);
                foreach (RayPerceptionOutput.RayOutput rayOutput in r3.RayOutputs)
                {
                    if (rayOutput.HasHit && rayOutput.HitGameObject.CompareTag("Goal"))
                    {
                        if (rayOutput.HitFraction < 0.04f)
                        {
                            Debug.Log("Is close enough to Goal" + rayOutput.HitFraction);
                            AddReward((0.1f / MaxStep));
                        }

                    }

                    if (rayOutput.HasHit && rayOutput.HitGameObject.CompareTag("Killer"))
                    {
                        
                        if (rayOutput.HitFraction < 0.04f)
                        {
                            Debug.Log("DANGER! Close to Killer" + rayOutput.HitFraction);
                            AddReward((0.1f / MaxStep));
                        }
                    }
                }
            }
            
                            Debug.Log("Current rewards" + GetCumulativeReward());
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            //Debug.Log("Entering Heuristics:");
            ActionSegment<float> continousActions = actionsOut.ContinuousActions;
            continousActions[0] =  _input.Cyclic.y;
            continousActions[1] = -_input.Cyclic.x;
            continousActions[2] = _input.Pedals;
            continousActions[3] = _input.Throttle;

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Goal>(out Goal goal))
            {
                Debug.Log("Collided with" + other);
                SetReward(+1f); //reward value is only relative to other rewards
                EndEpisode();
            }
        
            if (other.TryGetComponent<Ground>(out Ground ground))
            {
             
                Debug.Log("Collided with " + other);
                //SetReward(-1f);
            }

            if (other.TryGetComponent<Killer>(out Killer killer))
            {
                Debug.Log("Collided with " + other);
                SetReward(-1f);
                EndEpisode();
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
            foreach (IEngine _engine in _engines)
            {
                _engine.UpdateEngine(rb,_input);
            }
        }

        /*
        protected virtual void HandleControls()
        {
             _pitch = _input.Cyclic.y * minMaxPitch;
             _roll = -_input.Cyclic.x * minMaxRoll;
            _yaw += _input.Pedals * yawPower;

            _finalPitch = Mathf.Lerp(_finalPitch, _pitch, Time.deltaTime * lerpSpeed);
            _finalRoll = Mathf.Lerp(_finalRoll, _roll, Time.deltaTime * lerpSpeed);
            _finalYaw = Mathf.Lerp(_finalYaw, _yaw, Time.deltaTime * lerpSpeed);
            
            Quaternion rot = Quaternion.Euler(_finalPitch,_finalYaw,_finalRoll);
            //Add torque later
            rb.MoveRotation(rot);
        }
        */
        #endregion
        
    }
    
}
