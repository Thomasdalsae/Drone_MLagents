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
        transform.localScale = new Vector3(Random.Range(0.2f, 1.5f), Random.Range(3f, 20f), Random.Range(0.5f, 1f));
        rotationSpeed = (Random.Range(20f, 50f));
    }   

    private void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
