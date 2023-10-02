using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.Serialization;


namespace TdsWork
{
    [RequireComponent(typeof(Drone_Inputs))]
    public class Drone_Controller : Base_RigidBody
    {
        #region Variables

        [Header("Control Properties")] 
        [SerializeField]private float minMaxPitch = 30f;
        [SerializeField]private float minMaxRoll = 30f;
        [SerializeField]private float yawPower = 4f;
        [SerializeField] private float lerpSpeed = 2f;

        [Header("Ml Targets")] [SerializeField]
        private GameObject goal;
        //private Transform _targetTransform;
        
        
        private Drone_Inputs _input;
        private List<IEngine> _engines = new List<IEngine>();

        private float _pitch;
        private float _finalPitch;
        private float _roll;
        private float _finalRoll;
        private float _yaw;
        private float _finalYaw;
        #endregion

        #region Main Methods

        private void Start()
        {
            _input = GetComponent<Drone_Inputs>(); // grab the instance of the drone inputs;
            _engines = GetComponentsInChildren<IEngine>().ToList<IEngine>();
        }

        #endregion

        #region ML

        public override void OnEpisodeBegin()
        {
            
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            //sensor.AddObservation(_targetTransform.gameObject.transform.localPosition);
            Vector3 DirToGoal = (goal.transform.localPosition - transform.localPosition).normalized;
            sensor.AddObservation(DirToGoal.x);
            sensor.AddObservation(DirToGoal.y);
            sensor.AddObservation(DirToGoal.z);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {

            float pitch = actions.ContinuousActions[0];
            float roll = actions.ContinuousActions[1];
            _yaw =+ actions.ContinuousActions[2];
            
            HandleControls(_pitch,_roll,_yaw);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            ActionSegment<float> continousActions = actionsOut.ContinuousActions;
            continousActions[0] =  _input.Cyclic.y * minMaxPitch;
            continousActions[1] = -_input.Cyclic.x * minMaxRoll;
            continousActions[2] = _input.Pedals * yawPower;
            
        }
        

        #endregion
        #region Custom Methods

        protected override void HandlePhysics() //This will not run if there is no rigidbody in rb script,
        {
            HandleEngines();
            HandleControls(_pitch,_roll,_yaw);
        }

        protected virtual void HandleEngines()
        {
            //rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude));
            foreach (IEngine _engine in _engines)
            {
                _engine.UpdateEngine(rb,_input);
            }
        }

        protected virtual void HandleControls(float pitch,float roll,float _yaw)
        {
             pitch = _input.Cyclic.y * minMaxPitch;
             roll = -_input.Cyclic.x * minMaxRoll;
            _yaw += _input.Pedals * yawPower;

            _finalPitch = Mathf.Lerp(_finalPitch, pitch, Time.deltaTime * lerpSpeed);
            _finalRoll = Mathf.Lerp(_finalRoll, roll, Time.deltaTime * lerpSpeed);
            _finalYaw = Mathf.Lerp(_finalYaw, _yaw, Time.deltaTime * lerpSpeed);
            
            Quaternion rot = Quaternion.Euler(_finalPitch,_finalYaw,_finalRoll);
            //Add torque later
            rb.MoveRotation(rot);
        }
        #endregion
    }
}
