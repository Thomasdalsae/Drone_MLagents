using System;
using System.Collections;
using System.Collections.Generic;
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

 private void TrackCheckpoints_OnDroneCorrectCheckpoint(object sender, System.EventArgs e)
 {
  Hide();
 }
 private void TrackCheckpoints_OnDroneWrongCheckpoint(object Sender, System.EventArgs e)
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
