using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
//attribution: https://youtu.be/hH0OYz7YtKk?feature=shared
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private float _speed;
    [SerializeField] private bool _isCircuit; // whether it will treat waypoints[]
                                            //as a circle or a line (will be the same for two)
    private Transform _targetWaypoint;
    private int _currentWaypointIndex=0;
    private float _checkDistance = 0.05f;
    private bool _frontwards = true; //for linear movement

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

    private Transform getNextWPCircuit() { // waypoints are in a circuit (a,b,c,a,b,c)
        _currentWaypointIndex++;
        if(_currentWaypointIndex >= _waypoints.Length) {
            _currentWaypointIndex = 0;
        }
        return _waypoints[_currentWaypointIndex];
    }
    private Transform getNextWPLinear() { // waypoints are in a line (a,b,c,b,a,b)
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

    private void OnCollisionEnter2D(Collision2D other) { // player standing on platform
        var playerHandler = other.collider.GetComponent<PlayerHandler>();
        if(playerHandler != null) {
            if(playerHandler.isGrounded() && transform.position.y < playerHandler.transform.position.y) { //TODO this needs to be more robust, check if the player is grounded on this object!
                playerHandler.setParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other) { // player leaving platform
        var playerHandler = other.collider.GetComponent<PlayerHandler>();
        if(playerHandler != null) {
                playerHandler.resetParent();
            playerHandler.addVelocity(GetComponent<Rigidbody2D>().velocity);
        }
    }
}
