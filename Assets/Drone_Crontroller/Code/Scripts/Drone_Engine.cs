using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TdsWork
{
    [RequireComponent(typeof(BoxCollider))]
    public class Drone_Engine : MonoBehaviour, IEngine
    {
        #region Variables
        [Header("Engine Properties")] 
        [SerializeField]private float maxPower = 4f;
        #endregion Variables
        #region Interface Methods

        public void InitEngine()
        {
            throw new NotImplementedException();
        }

        public void UpdateEngine(Rigidbody rb, Drone_Inputs input)
        {
            //Debug.Log("Running Engine: " + gameObject.name);
            
            Vector3 engineForce = Vector3.zero;
            engineForce = transform.up * ((rb.mass * Physics.gravity.magnitude) + (input.Throttle * maxPower)) / 4f; // 4f since we got 4 engines
            rb.AddForce(engineForce, ForceMode.Force);
        }

        #endregion
    }
}
