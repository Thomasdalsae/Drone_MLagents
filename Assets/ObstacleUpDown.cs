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
        transform.localPosition = startLocation + new Vector3(0, Random.Range(-4f, 6f), Random.Range(-3f, 3f));
        transform.localScale = new Vector3(Random.Range(60f, 70f), Random.Range(8f, 15f), Random.Range(0.5f, 3f));
        
        rotationSpeed = (Random.Range(10f, 15f));
    }

    // Update is called once per frame
    void Update()
    {
        VerticalLimit += Time.deltaTime;

        //transform.Rotate(-Vector3.forward * rotationSpeed * Time.deltaTime);
        // Change direction when the VerticalLimit exceeds a threshold (e.g., 5 seconds)
        if (VerticalLimit >= 6f)
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
