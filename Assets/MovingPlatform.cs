using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private float _speed;
    [SerializeField] private bool _isCircuit;
    private Transform _targetWaypoint;
    private int _currentWaypointIndex=0;
    private float _checkDistance = 0.05f;
    private bool _frontwards = true;

    // Start is called before the first frame update
    void Start()
    {
        _targetWaypoint = _waypoints[0];
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            _targetWaypoint.position,
            _speed * Time.deltaTime);

        if(Vector2.Distance(transform.position, _targetWaypoint.position) < _checkDistance) {
            if(_isCircuit) {
                _targetWaypoint = getNextWPCircuit();
            } else {
                _targetWaypoint = getNextWPLinear();
            }
            
        }

    }

    private Transform getNextWPCircuit() {
        _currentWaypointIndex++;
        if(_currentWaypointIndex >= _waypoints.Length) {
            _currentWaypointIndex = 0;
        }
        return _waypoints[_currentWaypointIndex];
    }
    private Transform getNextWPLinear() {
        if(_frontwards) {
            _currentWaypointIndex++;
        }else{
            _currentWaypointIndex--;
        }
        if(_currentWaypointIndex >= _waypoints.Length) {
            _currentWaypointIndex -=2;
            _frontwards = false;
        }
        if(_currentWaypointIndex < 0) {
            _currentWaypointIndex +=2;
            _frontwards = true;
        }
        return _waypoints[_currentWaypointIndex];
    }

    private void OnCollisionEnter2D(Collision2D other) {
        var playerHandler = other.collider.GetComponent<PlayerHandler>();
        if(playerHandler != null) {
            playerHandler.setParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D other) {
        var playerHandler = other.collider.GetComponent<PlayerHandler>();
        if(playerHandler != null) {
            playerHandler.resetParent();
        }
    }
}
