using System;
using System.Collections;
using System.Collections.Generic;
using TdsWork;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class CheckpointSingle : MonoBehaviour
{
    private TrackCheckpoints _trackCheckpoints;
    private MeshRenderer _meshRenderer;
 public float rotationSpeed = 5f;
    public float radius = 2f;
    public float directionChangeTime = 5f;

    private float currentAngle = 0f;
    private float elapsedTime = 0f;
    private int rotationDirection = 1; // 1 for clockwise, -1 for counterclockwise
    private Vector3 initialLocalPosition;




    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    { 
        // Store the initial local position
        initialLocalPosition = transform.localPosition;
        directionChangeTime = Random.Range(0, 0);
        rotationSpeed = Random.Range(0f, 0f);
        
        // Randomly choose the initial rotation direction
       // rotationDirection = Random.Range(0, 2) * 2 - 1; // Randomly sets it to either 1 or -1
        transform.localPosition = transform.localPosition +  new Vector3(Random.Range(-0f, 0f), Random.Range(0f, 0f), 0);
       Hide(); 
    }

    void Update()
    {
  // Update the elapsed time
        elapsedTime += Time.deltaTime;

        // Change direction after a certain time
        if (elapsedTime >= directionChangeTime)
        {
            // Reset elapsed time
            elapsedTime = 0f;

            // Change rotation direction
            rotationDirection *= -1;
        }

        // Calculate the new position on the circle
        currentAngle += Time.deltaTime * rotationSpeed * rotationDirection;
        float x = Mathf.Cos(currentAngle) * radius;
        float z = Mathf.Sin(currentAngle) * radius;

        // Smoothly interpolate between the current position and the target position
        Vector3 targetPosition = initialLocalPosition + new Vector3(x, z, 0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 5f);
    }
      
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Drone>(out Drone drone))
        {
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

    public void Hide()
    {
        _meshRenderer.enabled = false;
    }
    // Draw a forward vector in the scene view for visualization
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward);
        }
}
