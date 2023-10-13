using System;
using System.Collections;
using System.Collections.Generic;
using TdsWork;
using Unity.VisualScripting;
using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    private TrackCheckpoints _trackCheckpoints;
    private MeshRenderer _meshRenderer;


    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
       Hide(); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Drone>(out Drone drone))
        {

            
            Debug.Log("Ontriggerenter: " + this,drone.transform);
            _trackCheckpoints.DroneThroughCheckpoint(this,drone.transform);
        } 
    }

    public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints)
    {
        this._trackCheckpoints = trackCheckpoints;
    }

    public void Show()
    {
        
        _meshRenderer.enabled = true;
    }

    private void Hide()
    {
        _meshRenderer.enabled = false;
    }
}
