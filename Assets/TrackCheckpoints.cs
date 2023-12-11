using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class TrackCheckpoints : MonoBehaviour
{
    public event EventHandler<DroneCheckPointEventArgs> OnDroneCorrectCheckpoint;
    public event EventHandler<DroneCheckPointEventArgs> OnDroneWrongCheckpoint;
    
    public event EventHandler<DroneCheckPointEventArgs> OnDroneLastCheckpoint;

    [SerializeField] private List<Transform> DroneTransformList;
    private List<CheckpointSingle> checkpointSingleList;
    private List<int> nextCheckpointSingleIndexList;
    private TrackCheckpointUI _trackCheckpointUI;
    
    
      public class DroneCheckPointEventArgs : EventArgs
       {
           public Transform  droneTransform { get; set; }
       } 
    
    private void Awake()
    {
        Transform checkpointsTransform = transform.Find("Checkpoints");

        _trackCheckpointUI = GetComponent<TrackCheckpointUI>();

        checkpointSingleList = new List<CheckpointSingle>();
        foreach (Transform checkpointSingleTransform in checkpointsTransform)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
            checkpointSingle.SetTrackCheckpoints(this);

            checkpointSingleList.Add(checkpointSingle);
        }

        nextCheckpointSingleIndexList = new List<int>();

        foreach (var droneTransform  in DroneTransformList)
        {
            nextCheckpointSingleIndexList.Add(0);
        }

    }

   
public void DroneThroughCheckpoint(CheckpointSingle checkpointSingle, Transform droneTransform)
{
    int nextCheckpointSingleIndex = nextCheckpointSingleIndexList[DroneTransformList.IndexOf(droneTransform)];

    if (checkpointSingleList.IndexOf(checkpointSingle) == nextCheckpointSingleIndex)
    {
        // Correct Checkpoint
        Debug.Log("Correct");
        CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
        correctCheckpointSingle.Show();

        // Update the next checkpoint index if i want the drone to fly more than one round on the track
        
        // add the next checkpoint index to the list til the drone reaches the last checkpoint
        nextCheckpointSingleIndex++;
        
        nextCheckpointSingleIndexList[DroneTransformList.IndexOf(droneTransform)] = (nextCheckpointSingleIndex);

        // if the drone reaches the last checkpoint
        if (nextCheckpointSingleIndexList[DroneTransformList.IndexOf(droneTransform)] == checkpointSingleList.Count)
        {
            // Last Checkpoint
            Debug.Log("Last");
            OnDroneLastCheckpoint?.Invoke(this, new DroneCheckPointEventArgs
            {
                droneTransform = droneTransform
            });
        }
       
        

        // Create an instance of DroneCheckPointEventArgs with the droneTransform data
        var eventArgs = new DroneCheckPointEventArgs
        {
            droneTransform = droneTransform
        };
        

        // Use the custom event arguments class
        OnDroneCorrectCheckpoint?.Invoke(this, eventArgs);
        
        
    }
    else
    {
        // Wrong CheckPoint
        Debug.Log("Wrong");

        // Create an instance of DroneCheckPointEventArgs with the droneTransform data
        var eventArgs = new DroneCheckPointEventArgs
        {
            droneTransform = droneTransform
        };

        // Use the custom event arguments class
        OnDroneWrongCheckpoint?.Invoke(this, eventArgs);

        CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
        correctCheckpointSingle.Show();
    }
}

   

     public void ResetCheckPoint(Transform droneTransform)
     {

         foreach (var checkpoint in checkpointSingleList)
         {
             checkpoint.Hide();
         }
         
         nextCheckpointSingleIndexList[DroneTransformList.IndexOf(droneTransform)] = 0; // <<< check later
         
         
     }

    public CheckpointSingle GetNextCheckpointPosition(Transform droneTransform)
    {
        int nextCheckpointSingleIndex = nextCheckpointSingleIndexList[DroneTransformList.IndexOf(droneTransform)];
        return checkpointSingleList[nextCheckpointSingleIndex];
    }

    public Vector3 GetNextCheckpointlocation(Transform droneTransform)
    {
        int nextCheckpointSingleIndex = nextCheckpointSingleIndexList[DroneTransformList.IndexOf(droneTransform)];
         Vector3 CurrentCheckpointLocation = checkpointSingleList[nextCheckpointSingleIndex].transform.localPosition; // <<<<<

         return CurrentCheckpointLocation;
    }
    
    
}
