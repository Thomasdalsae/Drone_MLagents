using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
        ThisGoal = Goal;
    }

    public void ResetGoal()
    {
        GoalHasSpawned = false;
    }

    public void SpawnFood()
    {
        _spawnRngPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(0.0f, 10f), Random.Range(-10f, 10f));
        Debug.Log("SpawnFood Gets called");
        ThisGoal = Instantiate(Goal.gameObject, _spawnRngPosition, quaternion.identity);
        GoalHasSpawned = true;
    }

    public Vector3 GetLastGoalTransform()
    {
        _lastpostion = ThisGoal.gameObject.transform.localPosition;

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
        Destroy(ThisGoal.gameObject);
        GoalHasSpawned = false;
    }
}
