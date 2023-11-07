using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NewBehaviourScript : MonoBehaviour
{
    #region Settings

    [SerializeField] private float Speed;
    [SerializeField] private float Limit = 1;
    [SerializeField] private Vector3 startLocation;
    #endregion


    
    private void Start()
    {
        startLocation = transform.localPosition;
        transform.localPosition = transform.localPosition +  new Vector3(0, Random.Range(-1f, 1f),  Random.Range(-1f, 1f));
        transform.localScale = new Vector3(Random.Range(10f, 15f), Random.Range(0.5f, 1f), Random.Range(0.3f, 1.2f));
    }

    // Update is called once per frame
    void Update()
    {
        Limit += Time.deltaTime;
        if (Limit >= 4f)
        {
            transform.localPosition += new Vector3(0,-Speed * Time.deltaTime, 0); 
        }
        else
        {
            
            transform.localPosition += new Vector3(0,Speed * Time.deltaTime, 0); 
        }

        if (Limit >= 8)
        {
            
        Limit = 0;
        }
    }
}
