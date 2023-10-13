using System;
using UnityEngine;

public class TrackCheckpointUI : MonoBehaviour
{
    [SerializeField] private TrackCheckpoints _trackCheckpoints;

    private void Start()
    {
        _trackCheckpoints.OnDroneCorrectCheckpoint += TrackCheckpoints_OnDroneCorrectCheckpoint;
        _trackCheckpoints.OnDroneCorrectCheckpoint += TrackCheckpoints_OnDroneWrongCheckpoint;

        Hide();
    }

    private void TrackCheckpoints_OnDroneCorrectCheckpoint(object sender, TrackCheckpoints.DroneCheckPointEventArgs e)
    {
        Hide();
    }

    private void TrackCheckpoints_OnDroneWrongCheckpoint(object Sender, TrackCheckpoints.DroneCheckPointEventArgs e)
    {
        Show();
    }


    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

  
    
}