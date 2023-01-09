using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Enemy : MonoBehaviour
{
    #region VARIABLES

    [Header("Damage & Health")]
    [SerializeField] private float _health; // Might not be required
    [SerializeField] private float _damage; // Amount of damage inflicted on Station

    [Header("Speed")]
    public float minSpeed;              // Minimum speed at which the enemy can go at on spawn
    public float maxSpeed;              // Maximum speed at which the enemy can go at on spawn
    public float gravityFactor;         // Speed at which the enemy accelerates when approaching the Station's orbit
    private float _speed;               // Speed at which the enemy travels towards the Station
    
    [Header("Physics")]
    private Vector2 _CenterPosition;    // For us, this is (0,0), i.e., the position of the Station
    private Vector2 _Direction;         // Direction between this enemy and the center position
    public float _distance;            // Distance between this enemy and the center position

    [Header("CollisionDetection")]
    [SerializeField] private float _largeShieldDistance;    // Distance by which an enemy is considered to have collided with the LargeShield
    [SerializeField] private float _smallShieldDistance;    // Distance by which an enemy is considered to have collided with the SmallShield
    [SerializeField] private float _stationHQDistance;      // Distance by which an enemy is considered to have collided with the StationHQ
    [SerializeField] private float _collisionOffset;        // This is to avoid destroying enemies which have already penetrated the shield

    [Header("Animations")]
    [SerializeField] private float _explosionSpeed;

    [Header("Debug")]
    public bool freeze;
    public bool dontDestroy;

    #endregion VARIABLES

    #region METHODS
    #region PRIVATE

    // When Enemy is instantiated
    private void Awake ()
    {
        // Apply a random speed on instantiation and calculate distance to center
        _speed = Random.Range(minSpeed, maxSpeed);
        _distance = Vector2.Distance(transform.position, _CenterPosition);

        // Calculate the direction between the GameObject and the central point (i.e., the Station)
        _Direction = _CenterPosition - new Vector2(transform.position.x, transform.position.y);
        _Direction.Normalize();

        // 
    }

    // Update is called once per frame
    private void Update ()
    {
        if (!freeze) {
            MoveTowardsStation();
        }
        IncreaseSpeedOverDistance();
    }

    // Used for physics updates (such as positional calculations)
    private void FixedUpdate ()
    {
        if (!dontDestroy)
            DestroyOnDistance();
    }

    // Constantly move towards the Station's position
    private void MoveTowardsStation ()
    {
        Vector2 currentPosition = transform.position;
        currentPosition += _speed * Time.deltaTime * _Direction;
        transform.position = currentPosition;
    }

    // Increase speed as distance between enemy and target reduces (gravitational effect)
    private void IncreaseSpeedOverDistance ()
    {
        _distance = Vector2.Distance(transform.position, _CenterPosition);
        _speed += (gravityFactor / _distance) * Time.deltaTime;
    }

    // Manual "collision" detection by checking the distance between enemy and target (center), and destroying enemy is within specific range
    private void DestroyOnDistance ()
    {
        if (Station.GetInstance().CheckElementState(Station.StationElement.LargeShield)) {
            if (_largeShieldDistance - _collisionOffset < _distance && _distance < _largeShieldDistance) {
                Station.GetInstance().HandleCollision(Station.StationElement.LargeShield, _damage);
                Destroy(this.gameObject);
            }
        }

        if (Station.GetInstance().CheckElementState(Station.StationElement.SmallShield)) {
            if (_smallShieldDistance - _collisionOffset < _distance && _distance < _smallShieldDistance) {
                Station.GetInstance().HandleCollision(Station.StationElement.SmallShield, _damage);
                Destroy(this.gameObject);
            }
        }

        if (_distance < _stationHQDistance) {
            Station.GetInstance().HandleCollision(Station.StationElement.StationHQ, _damage);
            Destroy(this.gameObject);
        }
    }

    // Destruction animation
    private IEnumerator Explode ()
    {
        float time = 0;
        while (time < _explosionSpeed) {
            yield return null;
        }

    }

    #endregion PRIVATE

    #endregion METHODS
}
