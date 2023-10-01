using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        private Drone_Inputs _inputs;
        #endregion

        #region Main Methods

        private void Start()
        {
            _inputs = GetComponent<Drone_Inputs>(); // grab the instance of the drone inputs;
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
            rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude));
        }

        protected virtual void HandleControls()
        {
            
        }
        #endregion
    }
}
