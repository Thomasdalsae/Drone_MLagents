using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;


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

        private Drone_Inputs _input;
        private List<IEngine> _engines = new List<IEngine>();

        private float _finalPitch;
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

        #region Custom Methods

        protected override void HandlePhysics() //This will not run if there is no rigidbody in rb script,
        {
            HandleEngines();
            HandleControls();
        }

        protected virtual void HandleEngines()
        {
            //rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude));
            foreach (IEngine _engine in _engines)
            {
                _engine.UpdateEngine(rb,_input);
            }
        }

        protected virtual void HandleControls()
        {
            float pitch = _input.Cyclic.y * minMaxPitch;
            float roll = -_input.Cyclic.x * minMaxRoll;
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
