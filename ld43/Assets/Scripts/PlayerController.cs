using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour {
    public Canvas Canvas;
    public Camera PlayerCamera;
    public GameLevelManager LevelManager;
    public LeaderController Leader;
    public AudioSource WitnessMeSFX;
    public AudioSource BlipYesSFX;
    public AudioSource BlipNoSFX;
    public AudioSource[] ExplosionSFX;
    public NavMeshAgent[] PlayerAgents;
    public GameObject SelectionCircle;
    public GameObject DestroyedPrefab;

    private bool _firstMove = true;
    private Vector3 _lastTarget = Vector3.zero;

    private NavMeshAgent _selectedAgent;

    private List<Vector3> _targetsUnrotated = new List<Vector3>();
    private List<Vector3> _targetsRotated = new List<Vector3>();
    private List<Vector3> _targetsRaycast = new List<Vector3>();
    
    public NavMeshAgent GetCommander() {
        if (PlayerAgents.Length > 0) {
            return PlayerAgents[0];
        }

        return null;
    }

    void Start() {
        // Hack for development - without the Intro UI, we'll never leave the Intro state
        if (!Canvas.isActiveAndEnabled && (LevelManager.State == GameLevelManager.GameState.Intro)) {
            LevelManager.ChangeState(GameLevelManager.GameState.Playing);
        }
        
        //
        SelectionCircle.SetActive(false);

        foreach (NavMeshAgent navMeshAgent in PlayerAgents) {
            PlayerAgent agent = navMeshAgent.gameObject.GetComponent<PlayerAgent>();
            agent.Player = this;
        }
    }

    private bool _attackCancelled = false;
    IEnumerator WaitForAttackCompleted() {
        _attackCancelled = false;
        
        bool completed = false;
        do {
            yield return null;
            
            if (_selectedAgent != null) {
                if (!_selectedAgent.pathPending) {
                    if (_selectedAgent.remainingDistance <= _selectedAgent.stoppingDistance) {
                        if (!_selectedAgent.hasPath || (_selectedAgent.velocity.sqrMagnitude < Mathf.Epsilon)) {
                            completed = true;
                        }
                    }
                }
            }
        } while (!completed || _attackCancelled);

        // Return to Playing state
        LevelManager.ChangeState(GameLevelManager.GameState.Playing);
    }

    public void CancelAttack() {
        _attackCancelled = true;
    }
    
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            _targetsUnrotated.Clear();
            _targetsRotated.Clear();
            _targetsRaycast.Clear();
            
//            Debug.Log($"Current state: {LevelManager.State}");
            
            Ray mouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            
            int layerMask = LayerMask.GetMask("Player", "Enemies", "Ground", "Sacrifices", "Targets");

            RaycastHit targetHit;
            if (Physics.Raycast(mouseRay, out targetHit, Mathf.Infinity, layerMask)) {
                int hitLayer = targetHit.collider.gameObject.layer;
                
//                Debug.Log($"Hit layer: {LayerMask.LayerToName(hitLayer)} ({hitLayer})");
                
                if (hitLayer == LayerMask.NameToLayer("Ground")) {
                    if (LevelManager.State == GameLevelManager.GameState.Playing) {
                        BlipYesSFX.Play();
                        MoveBattalion(targetHit);
                    }
                } else if (hitLayer == LayerMask.NameToLayer("Player")) {
                    if (LevelManager.State == GameLevelManager.GameState.Attacking) {
                        // Select the agent
                        PlayerAgent agent = targetHit.collider.gameObject.GetComponentInParent<PlayerAgent>();
                        NavMeshAgent navMeshAgent = targetHit.collider.gameObject.GetComponentInParent<NavMeshAgent>();
                        if ((agent != null) && (!agent.IsCommander || (PlayerAgents.Length == 1)) && (navMeshAgent != null)) {
                            _selectedAgent = navMeshAgent;
                            
                            BlipYesSFX.Play();
                        }
                        else {
                            BlipNoSFX.Play();
                        }
                    }
                } else if ((hitLayer == LayerMask.NameToLayer("Enemies")) ||
                           (hitLayer == LayerMask.NameToLayer("Sacrifices")) ||
                           (hitLayer == LayerMask.NameToLayer("Targets"))) {
                    // Attack the enemy
                    if (LevelManager.State == GameLevelManager.GameState.Attacking) {
                        if (_selectedAgent != null) {
                            WitnessMeSFX.PlayDelayed(0.2f);

                            _selectedAgent.SetDestination(targetHit.point);
                            StartCoroutine(WaitForAttackCompleted());
                        }
                    }
                }
                else {
                    BlipNoSFX.Play();
                }
            }
            else {
                BlipNoSFX.Play();
            }
        }
    }

    private void LateUpdate() {
        if ((LevelManager.State == GameLevelManager.GameState.Attacking) && (_selectedAgent != null)) {
            SelectionCircle.transform.position = new Vector3(_selectedAgent.transform.position.x, 0.0f, _selectedAgent.transform.position.z);
            SelectionCircle.SetActive(true);
        }
        else {
            SelectionCircle.SetActive(false);
        }
    }

    private void MoveBattalion(RaycastHit targetHit) {
        // Formation:
        // O O O
        // O O O O
        // O O O
        // 

        // Find the dXd formation that's needed for the number of agents we have active
        int numAgents = PlayerAgents.Length;
        int dZ = Mathf.CeilToInt(Mathf.Sqrt(numAgents));
        int dX = Mathf.CeilToInt((float) numAgents / (float) dZ);
        // The front line will have fewer agents than those that follow it, if necessary
        int dFrontLine = numAgents % dX;
        int dXHalf = (dX + 1) / 2;
        int dX0Half = (dFrontLine + 1) / 2;
        int dZHalf = (dZ + 1) / 2;

        float radius = 6.0f;

        // Rotate formation about Y-axis by projection of (newTarget - prevTarget) on XZ plane
        Vector3 movementVector = targetHit.point - _lastTarget;
        if (!_firstMove) {
            // If moving within the radius of the current formation, just re-orient to the camera rather than
            // the delta between moves, as it could be rather random at smaller magnitudes
//            if (movementVector.magnitude < (radius * dXHalf)) {
//                movementVector = PlayerCamera.transform.forward;
//            }
        }
        else {
            movementVector = PlayerCamera.transform.forward;
        }

        movementVector.y = 0.0f;
        movementVector.Normalize();

        float angle = Mathf.Atan2(movementVector.x, movementVector.z) * Mathf.Rad2Deg;
        Quaternion quatRot = Quaternion.AngleAxis(angle, Vector3.up);
        Matrix4x4 formationRotation = Matrix4x4.Rotate(quatRot);

//        Debug.Log($"Agent formation: {dX}x{dZ}. First line: {dFrontLine}. Angle: {angle}");

        _firstMove = false;
        _lastTarget = targetHit.point;

        for (int agentIdx = 0; agentIdx < PlayerAgents.Length; agentIdx++) {
            NavMeshAgent agent = PlayerAgents[agentIdx];

            bool isFrontLine = agentIdx < dFrontLine;
            int zIndex = (((PlayerAgents.Length - 1) - agentIdx) / dX) - dZHalf;
            int xIndex;
            if (isFrontLine) {
                xIndex = (agentIdx % dX) - dX0Half;
            }
            else {
                xIndex = ((agentIdx - dFrontLine) % dX) - dXHalf;
            }

            float zOffset = zIndex * radius;
            float xOffset = xIndex * radius;
            if (isFrontLine && ((dFrontLine % 2) == 1)) {
                xOffset += radius / 2.0f;
            }
            else if (!isFrontLine && ((dX % 2) == 1)) {
                xOffset += radius / 2.0f;
            }

            if ((dZ % 2) == 1) {
                zOffset += radius / 2.0f;
            }

            Vector3 offset = new Vector3(xOffset, 0.0f, zOffset);

            Vector3 rotatedOffset = formationRotation.MultiplyPoint3x4(offset);
            Vector3 desiredLocation = targetHit.point + rotatedOffset;

            Ray newRay = new Ray(PlayerCamera.transform.position, desiredLocation - PlayerCamera.transform.position);

            RaycastHit hit;
            if (!targetHit.collider.Raycast(newRay, out hit, targetHit.distance + offset.magnitude + 10.0f)) {
                hit = targetHit;
            }

            if (agent.isActiveAndEnabled) {
                agent.SetDestination(hit.point);
            }

            _targetsUnrotated.Add(targetHit.point + offset);
            _targetsRotated.Add(desiredLocation);
            _targetsRaycast.Add(hit.point);

//            string unitType = isFrontLine ? "Frontline" : "Support";
//            Debug.Log($"Agent {agentIdx} ({xIndex}, {zIndex}) @ {offset}. {unitType}");
        }

        //
        Leader.OnPlayerBattalionMove(targetHit.point, PlayerAgents.Length);
    }

    public void AgentOnTriggerEnter(PlayerAgent agent, Collider other) {
        if (!other.gameObject.activeInHierarchy) {
            return;
        }
        
        int layer = other.gameObject.layer;
        if ((layer == LayerMask.NameToLayer("Enemies")) ||
            (layer == LayerMask.NameToLayer("Sacrifices"))) {
            if ((_selectedAgent != null) && (_selectedAgent.gameObject.GetInstanceID() == agent.gameObject.GetInstanceID())) {
                _selectedAgent = null;
            } else {
                // Only instantiate debris (NavMesh Obstacle) if it wasn't the result of a sacrifice
                GameObject.Instantiate(DestroyedPrefab, agent.gameObject.transform.position,
                    agent.gameObject.transform.rotation, this.gameObject.transform);
            }

            agent.gameObject.SetActive(false);
            other.gameObject.SetActive(false);

            int sfx = Random.Range(0, ExplosionSFX.Length);
            AudioSource explosion = ExplosionSFX[sfx];
            explosion.Play();

            int activeCount = 0;
            foreach (NavMeshAgent navMeshAgent in PlayerAgents) {
                if (navMeshAgent.gameObject.activeInHierarchy) {
                    activeCount++;
                }
            }

            if (activeCount == 0) {
                LevelManager.ChangeState(GameLevelManager.GameState.Lost);
            }
        } else if (layer == LayerMask.NameToLayer("Targets")) {
            LevelManager.ChangeState(GameLevelManager.GameState.Won);
        }
    }

//    private void OnDrawGizmos() {
//        Gizmos.color = Color.red;
//        foreach (Vector3 p in targetsUnrotated)
//            Gizmos.DrawWireSphere(p, 0.5f);
//
//        Gizmos.color = Color.blue;
//        foreach (Vector3 p in targetsRotated)
//            Gizmos.DrawWireSphere(p, 0.5f);
//        
//        Gizmos.color = Color.green;
//        foreach (Vector3 p in targetsRaycast)
//            Gizmos.DrawWireSphere(p, 0.5f);
//    }
}
