using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TdsWork
{
    public class Drone_Engine : MonoBehaviour, IEngine
    {
        #region Variables
        [Header("Engine Properties")] private float maxPower = 4f;
        #endregion Variables
        #region Interface Methods

        public void InitEngine()
        {
            throw new NotImplementedException();
        }

        public void UpdateEngine()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
