using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;


public class ObstacleMovement : MonoBehaviour
{
    #region Settings

    [SerializeField] private float rotationSpeed = 60f;

    #endregion


    private void Start()
    {
        transform.localScale = new Vector3(Random.Range(0.2f, 1.3f), Random.Range(9f, 20f), Random.Range(0.5f, 0.9f));
        rotationSpeed = (Random.Range(10f, 20f));
    }   

    private void Update()
    {
        transform.Rotate(-Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
