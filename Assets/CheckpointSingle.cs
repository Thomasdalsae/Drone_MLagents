using System;
using System.Collections;
using System.Collections.Generic;
using TdsWork;
using Unity.VisualScripting;
using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    private TrackCheckpoints _trackCheckpoints;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Drone>(out Drone drone))
        {
            _trackCheckpoints.PlayerThroughCheckpoint(this);
        } 
    }

    public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints)
    {
        this._trackCheckpoints = trackCheckpoints;
    }
}
