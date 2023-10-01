using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_Input : MonoBehaviour
{

    private void OnMove(InputValue value)
    {
        Debug.Log(value.Get<float>());
        
    }
}

