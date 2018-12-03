using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LeaderController : MonoBehaviour {
    public NavMeshAgent LeaderAgent;
    public float MinimumSpeed = 4.0f;
    public float MaximumSpeed = 20.0f;
    
    private Vector3 _startLocation = Vector3.zero;
    private Vector3 _frontLineLocation = Vector3.zero;
    private bool _hasFrontLine = false;

    public void OnPlayerBattalionMove(Vector3 squadLocation, int squadSize) {
        if (_hasFrontLine) {
            // Distance is always measured from the initial location, so the Leader is always "advancing"
            Vector3 frontLineDirection = _frontLineLocation - _startLocation;
            Vector3 squadDirection = squadLocation - _startLocation;
            float distanceToFrontLine = frontLineDirection.magnitude;
            float distanceToSquad = squadDirection.magnitude;

            if (distanceToSquad > distanceToFrontLine) {
                Vector3 leaderToSquad = squadLocation - LeaderAgent.gameObject.transform.position;
                float leaderToSquadDistance = leaderToSquad.magnitude;

                float movementSpeed = Mathf.Clamp(leaderToSquadDistance / 20.0f, MinimumSpeed, MaximumSpeed);
                
                _frontLineLocation = squadLocation;
                
                LeaderAgent.speed = movementSpeed;
                LeaderAgent.SetDestination(squadLocation);
            }
        }
        else {
            _hasFrontLine = true;
            _frontLineLocation = squadLocation;
            LeaderAgent.SetDestination(squadLocation);
        }
    }

    // Start is called before the first frame update
    void Start() {
        _startLocation = LeaderAgent.gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
