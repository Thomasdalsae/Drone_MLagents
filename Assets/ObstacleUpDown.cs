using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NewBehaviourScript : MonoBehaviour
{
    #region Settings

    [SerializeField] private float Speed;
    [SerializeField] private float VerticalLimit = 1;
    [SerializeField] private Vector3 startLocation;
    [SerializeField] private float rotationSpeed = 60f;
    #endregion

    private bool movingUp = true;

    private void Start()
    {
        startLocation = transform.localPosition;
        transform.localPosition = startLocation + new Vector3(0, Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        transform.localScale = new Vector3(Random.Range(10f, 15f), Random.Range(0.5f, 1f), Random.Range(0.3f, 1.2f));
        
        rotationSpeed = (Random.Range(10f, 25f));
    }

    // Update is called once per frame
    void Update()
    {
        VerticalLimit += Time.deltaTime;

        transform.Rotate(-Vector3.forward * rotationSpeed * Time.deltaTime);
        // Change direction when the VerticalLimit exceeds a threshold (e.g., 5 seconds)
        if (VerticalLimit >= 2.5f)
        {
            movingUp = !movingUp;
            VerticalLimit = 0;
        }

        // Move the object up or down based on the movingUp flag
        if (movingUp)
        {
            transform.localPosition += new Vector3(0, Speed * Time.deltaTime, 0);
        }
        else
        {
            transform.localPosition += new Vector3(0, -Speed * Time.deltaTime, 0);
        }
    }
}
