using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

namespace TdsWork
{
    [RequireComponent(typeof(Rigidbody))]
    public class Base_RigidBody : Agent
    {
        #region Variables

        [Header("Rigidbody Properties")] [SerializeField]private float weightInKg = 1f;
        
        
        protected Rigidbody rb;
        protected float startDrag;
        protected float startAngularDrag;
        #endregion

        #region Main Methods

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb)
            {
                rb.mass = weightInKg;
                startDrag = rb.drag;
                startAngularDrag = rb.angularDrag;
            }
        }

        private void FixedUpdate()
        {
            if (!rb)
            {
                return;
            }

            HandlePhysics();
        }

        #endregion
        
        #region Custom Methods
        protected virtual void HandlePhysics(){}
    
        #endregion
    }
}
