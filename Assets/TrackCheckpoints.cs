using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class TrackCheckpoints : MonoBehaviour
{
    public event EventHandler<DroneCheckPointEventArgs> OnDroneCorrectCheckpoint;
    public event EventHandler<DroneCheckPointEventArgs> OnDroneWrongCheckpoint;

    [SerializeField] private List<Transform> DroneTransformList;
    private List<CheckpointSingle> checkpointSingleList;
    private List<int> nextCheckpointSingleIndexList;
    
    
      public class DroneCheckPointEventArgs : EventArgs
       {
           public Transform droneTransform{ get; set; }
       } 
    
    private void Awake()
    {
        Transform checkpointsTransform = transform.Find("Checkpoints");

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
            //Correct Checkpoint
            Debug.Log("Correct");
            CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
            correctCheckpointSingle.Show();
            
            nextCheckpointSingleIndexList[DroneTransformList.IndexOf(droneTransform)] = (nextCheckpointSingleIndex + 1) % checkpointSingleList.Count;
            //OnDroneCorrectCheckpoint?.Invoke(this, (DroneCheckPointEventArgs)EventArgs.Empty); //<--- Check later
            OnDroneCorrectCheckpoint?.Invoke(this, DroneCheckPointEventArgs.Empty as DroneCheckPointEventArgs); //<--- Check later
        }
        else
        {
            //Wrong CheckPoint
            Debug.Log("Wrong");
            OnDroneWrongCheckpoint?.Invoke(this, DroneCheckPointEventArgs.Empty as DroneCheckPointEventArgs); // <----Check Later

            CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
            correctCheckpointSingle.Show();
        }
    }
   

     public void ResetCheckPoint(Transform droneTransform)
     {
         nextCheckpointSingleIndexList[DroneTransformList.IndexOf(droneTransform)] = 0; // <<< check later
     }

    public CheckpointSingle GetNextCheckpointPosition(Transform droneTransform)
    {
        int nextCheckpointSingleIndex = nextCheckpointSingleIndexList[DroneTransformList.IndexOf(droneTransform)];
        return checkpointSingleList[nextCheckpointSingleIndex];
    }
}
