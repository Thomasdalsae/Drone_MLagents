using System;
using System.Collections;
using System.Collections.Generic;
using TdsWork;
using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Drone>(out Drone drone))
        {
            Debug.Log("Went pass ChadfgadfggadfadfggadfeckPoint");
        } 
    }
}
