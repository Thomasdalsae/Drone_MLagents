using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TdsWork
{   
    [RequireComponent(typeof(PlayerInput))] //Automatically check to see if a player input component is assigned to our game object,if not make one;
    public class Drone_Inputs : MonoBehaviour
    {
        #region Variables

        private Vector2 _cyclic;
        private float _pedals;
        private float _throttle;
        #endregion

        public Vector2 Cyclic { get => _cyclic; }
        public float Pedals { get => _pedals; }
        public float Throttle { get => _throttle; }
        
        #region Main Methods

        private void Update()
        {
            
            
        }

        #endregion

        #region Input Methods

        private void OnCyclic(InputValue value)
        {
            _cyclic = value.Get<Vector2>();
        }

        private void OnPedals(InputValue value)
        {
            _pedals = value.Get<float>();
        }

        private void OnThrottle(InputValue value)
        {
            _throttle = value.Get<float>();
        }

        #endregion Input Methods
        

    }
}
