using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region VARIABLES

    [Header("Damage & Health")]
    [SerializeField] private float _health; // Current shield's health
    [SerializeField] private float _damage; // Amount of damage inflicted on Station

    [Header("Speed")]
    public float minSpeed;              // Minimum speed at which the enemy can go at on spawn
    public float maxSpeed;              // Maximum speed at which the enemy can go at on spawn
    public float gravityFactor;         // Speed at which the enemy accelerates when approaching the Station's orbit
    private float _speed;               // Speed at which the enemy travels towards the Station

    [Header("Physics")]
    private Vector2 _CenterPosition;    // For us, this is (0,0), i.e., the position of the Station
    private Vector2 _Direction;         // Direction between this enemy and the center position
    public float _distance;             // Distance between this enemy and the center position

    [Header("CollisionDetection")]
    [SerializeField] private float _largeShieldDistance;    // Distance by which an enemy is considered to have collided with the LargeShield
    [SerializeField] private float _smallShieldDistance;    // Distance by which an enemy is considered to have collided with the SmallShield
    [SerializeField] private float _stationHQDistance;      // Distance by which an enemy is considered to have collided with the StationHQ
    [SerializeField] private float _collisionOffset;        // This is to avoid destroying enemies which have already penetrated the shield

    [Header("Animations")]
    [SerializeField] private float _explosionSpeed; // The same as the "Explosion" animation time, i.e., 2 seconds
    [SerializeField] private Animator _Animator;    // Animator component in charge of running the "Explosion" before destroying gameObject
    private bool _isExploding = false;              // Tracks whether the destruction coroutine has been started

    [Header("Debug")]
    [SerializeField] private bool _isFrozen;    // Stops enemy from moving
    [SerializeField] private bool _dontDestroy; // Stops enemy from dying

    #endregion VARIABLES

    #region METHODS
    #region UNITY

    // When Enemy is instantiated
    private void Awake ()
    {
        // Apply a random speed on instantiation and calculate distance to center
        _speed = Random.Range(minSpeed, maxSpeed);
    }

    // Just before the first frame before which the Enemy is istantiated
    private void Start ()
    {
        // Calculate the direction between the GameObject and the central point (i.e., the Station)
        _distance = Vector2.Distance(transform.position, _CenterPosition);
        _Direction = _CenterPosition - new Vector2(transform.position.x, transform.position.y);
        _Direction.Normalize();
    }

    // Update is called once per frame
    private void Update ()
    {
        if (!_isFrozen) {
            MoveTowardsStation();
        }
        IncreaseSpeedOverDistance();
    }

    // Used for physics updates (such as positional position and physics calculations)
    private void FixedUpdate ()
    {
        if (!_dontDestroy && !_isExploding)
            DestroyOnDistance();
    }

    #endregion UNITY

    #region PUBLIC

    // Manually start this enemy's explosion sequence (if for example it is hit by a weapon)
    public void ManualExplode ()
    {
        if (!_isExploding)
            StartCoroutine(Explode());
    }

    #endregion PUBLIC

    #region PRIVATE

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
        // Calculate distance remaining, and increase speed with an inverted ratio (simulating gravity effect)
        _distance = Vector2.Distance(transform.position, _CenterPosition);
        _speed += (gravityFactor / _distance) * Time.deltaTime;
    }

    // Manual "collision" detection by checking the distance between enemy and target (center), and destroying when enemy is within range
    private void DestroyOnDistance ()
    {
        // Kill all enemies when game is over
        if (Station.GetInstance().GetIsNova() && !_isExploding) {
            StartCoroutine(Explode());
        }

        // If Large Shield is active and alive, destroy when distance = _largeShieldDistance
        if (Station.GetInstance().CheckElementState(Station.StationElement.LargeShield)) {
            if (_largeShieldDistance - _collisionOffset < _distance && _distance < _largeShieldDistance) {
                // Two comparisions because if the shield reactivates once an enemy has passed it, we don't want to delete them
                Station.GetInstance().HandleCollision(Station.StationElement.LargeShield, _damage);
                StartCoroutine(Explode());
            }
        }

        // If Small Shield is active and alive, destroy when distance = _smallShieldDistance
        if (Station.GetInstance().CheckElementState(Station.StationElement.SmallShield)) {
            if (_smallShieldDistance - _collisionOffset < _distance && _distance < _smallShieldDistance) {
                Station.GetInstance().HandleCollision(Station.StationElement.SmallShield, _damage);
                StartCoroutine(Explode());
            }
        }

        // No matter what, destroy when distance < _stationHQDistance
        if (_distance < _stationHQDistance) {
            Station.GetInstance().HandleCollision(Station.StationElement.StationHQ, _damage);
            StartCoroutine(Explode());
        }
    }

    // Disable collision detection mechanics, launch destruction animation and destroy GameObject
    private IEnumerator Explode ()
    {
        // Remove this enemy from the game's list of current enemies in the scene
        GameManager.GetInstance().RemoveEnemy(this);

        // Start the explosion animation
        _isFrozen = true;
        _isExploding = true;
        GetComponent<CircleCollider2D>().enabled = false;
        _Animator.Play("Explosion");
        yield return new WaitForSeconds(_explosionSpeed);
        Destroy(gameObject);
    }

    #endregion PRIVATE

    #endregion METHODS
}
