using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GoalSpawner : MonoBehaviour
{
    [SerializeField] private GameObject Goal;
    [SerializeField] private GameObject ThisGoal;
    [SerializeField] private bool GoalHasSpawned;
    private Vector3 _lastpostion;
    private Vector3 _spawnRngPosition;

    // Start is called before the first frame update
    void Start()
    {
        //ResetGoal();
        transform.localPosition = new Vector3(0, 0, 0);
    }

    public void ResetGoal()
    {
        GoalHasSpawned = false;
    }

    public void SpawnFood()
    {
        //transform.localPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(1.0f, 10f), Random.Range(-7f, 7f));
        //transform.localPosition = new Vector3(Random.Range(-6f, 6f), Random.Range(3f, 10f), 7f);
        transform.localPosition = new Vector3(Random.Range(-6f, 6f), 4, 7f);
        //transform.localPosition = Vector3.zero; 
        Debug.Log("SpawnFood Gets called");
        ThisGoal = Instantiate(Goal,transform.gameObject.transform,false);
        GoalHasSpawned = true;
    }

    public Vector3 GetLastGoalTransform()
    {
        _lastpostion = gameObject.transform.localPosition;

        return _lastpostion;
    }

    public bool HasGoalSpawned()
    {
        if (GoalHasSpawned)
        {
            return true;
        }

        return false;
    }

    public void KillGoal()
    {
        Destroy(ThisGoal);
        GoalHasSpawned = false;
    }
}
